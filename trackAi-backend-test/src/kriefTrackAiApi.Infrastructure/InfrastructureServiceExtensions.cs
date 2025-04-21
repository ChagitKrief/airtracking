using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Repositories;
// using kriefTrackAiApi.UseCases.Services;
using kriefTrackAiApi.Infrastructure.Data;
using kriefTrackAiApi.Infrastructure.Extensions;

namespace kriefTrackAiApi.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration config,
      ILogger logger)
  {
    string? connectionString = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
      throw new ArgumentNullException(nameof(connectionString), "Connection string for PostgreSQL is missing.");
    }

    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddRepositories();

    logger.LogInformation("Infrastructure services registered successfully");

    return services;
  }
}
