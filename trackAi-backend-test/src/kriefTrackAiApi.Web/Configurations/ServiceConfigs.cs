using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure;
using kriefTrackAiApi.Infrastructure.Email;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace kriefTrackAiApi.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddInfrastructureServices(builder.Configuration, logger)
            .AddMediatrConfigs();


    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      // services.AddScoped<IEmailSender, MimeKitEmailSender>();
      services.AddSingleton<EmailService>();
      // Otherwise use this:
      //builder.Services.AddScoped<IEmailSender, FakeEmailSender>();

    }
    else
    {
      // services.AddScoped<IEmailSender, MimeKitEmailSender>();
      services.AddSingleton<EmailService>();
    }

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    return services;
  }


}
