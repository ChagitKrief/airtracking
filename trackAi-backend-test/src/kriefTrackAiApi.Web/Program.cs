using kriefTrackAiApi.Web.Configurations;
using kriefTrackAiApi.Infrastructure.Middleware;
using kriefTrackAiApi.Infrastructure.Services;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Extensions;
using kriefTrackAiApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Infrastructure.Data;
using kriefTrackAiApi.UseCases.Interfaces;
using kriefTrackAiApi.UseCases.Services;
using kriefTrackAiApi.Infrastructure.Repositories;
using kriefTrackAiApi.UseCases.Mappings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using kriefTrackAiApi.Infrastructure.Email;
using kriefTrackAiApi.Infrastructure.SmsService;
using kriefTrackAiApi.Web.Sockets;
using React.AspNet;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.ChakraCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

var smtpUser = builder.Configuration["EmailSettings:Username"]
    ?? throw new ArgumentNullException("EmailSettings:Username is missing");

var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new ArgumentNullException("Connection string is missing");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddServiceConfigs(appLogger, builder);


builder.Services.AddAutoMapper(typeof(MapProfile));

// SSR
builder.Services.AddSingleton<BlazorPrerenderService>();
builder.Services.AddSingleton<PrerenderCache>();
builder.Services.AddSingleton<BlazorPrerenderService>();
builder.Services.AddSingleton<PrerenderCache>();
builder.Services.AddRazorComponents();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ISmsRepository, SmsRepository>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddRepositories();
builder.Services.AddInfrastructureServices(builder.Configuration, appLogger);

//WinwordService
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<IWinwordRepository, WinwordDataMiddleware>();
builder.Services.AddScoped<WinwordDataMiddleware>();
builder.Services.AddScoped<WinwordFilterService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<WinwordQueryService>();

//map
builder.Services.AddSingleton<IContainerNotifier, ContainerNotifier>();
builder.Services.AddSignalR(options =>
{
  options.EnableDetailedErrors = true;
  options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddHostedService<ContainerTrackingService>();

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.WithOrigins()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(origin => true)
           .AllowCredentials();
  });
});

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(6060); // API
  options.ListenAnyIP(6001);  // WebSockets SignalR
});

//mail+sms services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<BackgroundTaskSync>();
builder.Services.AddHostedService<DailyShipmentJob>();
builder.Services.AddHostedService<ShipmentNotificationBackgroundService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<EmailWelcomeService>();
builder.Services.AddScoped<PasswordResetEmailService>();
// builder.Services.AddScoped<SmsNotificationService>();

builder.Services.AddControllers();

var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JwtSettings:Secret is missing");
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new ArgumentNullException("JwtSettings:Issuer is missing");
var audience = builder.Configuration["JwtSettings:Audience"] ?? throw new ArgumentNullException("JwtSettings:Audience is missing");
var expiryInMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpiryInMinutes");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
      };
    });

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("AdminOnly", policy => policy.RequireRole("1"));
  options.AddPolicy("AdminOrManager", policy => policy.RequireRole("1", "3"));
  options.AddPolicy("AllUsers", policy => policy.RequireRole("1", "2", "3"));
});

builder.Services.AddScoped<IAuthService>(provider =>
{
  var companyRepository = provider.GetRequiredService<ICompanyRepository>();
  var winwordQueryService = provider.GetRequiredService<WinwordQueryService>();
  var userRepository = provider.GetRequiredService<IUserRepository>();
  var emailService = provider.GetRequiredService<PasswordResetEmailService>();

  return new AuthService(
      jwtSecret,
      issuer,
      audience,
      expiryInMinutes,
      companyRepository,
      winwordQueryService,
      userRepository,
      emailService
  );

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

  var securityScheme = new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Description = "Enter 'Bearer {token}'",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    Reference = new OpenApiReference
    {
      Type = ReferenceType.SecurityScheme,
      Id = "Bearer"
    }
  };

  c.AddSecurityDefinition("Bearer", securityScheme);
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});


//Blazor 
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionManager>();


var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<ContainerTrackingHub>("/trackingHub"); // SignalR
app.MapFallbackToPage("/_Host");


using (var scope = app.Services.CreateScope())
{
  var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  // dbContext.Database.Migrate();
  dbContext.Database.CanConnect();

}

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
  });
}

// Middleware/OPTIONS (CORS)
app.Use(async (context, next) =>
{
  if (context.Request.Path.StartsWithSegments("/trackingHub") && context.Request.Method == "OPTIONS")
  {
    context.Response.StatusCode = 204;

    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,OPTIONS";
    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";

    return;
  }
  await next();
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapDefaultControllerRoute();

app.Run();

public partial class Program { }
