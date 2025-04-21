// using Ardalis.SharedKernel;
// using MediatR;
// using System.Reflection;

// namespace kriefTrackAiApi.Web.Configurations;

// public static class MediatrConfigs
// {
//   public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
//   {
//     var mediatRAssemblies = new[]
//       {
//         Assembly.GetAssembly(typeof(Contributor)), // Core
//         Assembly.GetAssembly(typeof(CreateContributorCommand)) // UseCases
//       };

//     services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
//             .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
//             .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

//     return services;
//   }
// }



using MediatR;
using System.Reflection;

namespace kriefTrackAiApi.Web.Configurations;

public static class MediatrConfigs
{
    public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        return services;
    }
}
