using Microsoft.Extensions.DependencyInjection;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Repositories;

namespace kriefTrackAiApi.Infrastructure.Extensions;

public static class RepositoryCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ISmsRepository, SmsRepository>();

        return services;
    }
}
