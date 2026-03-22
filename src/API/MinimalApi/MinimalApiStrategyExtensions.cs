using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Reflection;

namespace API.MinimalApi {
    public static class MinimalApiStrategyExtensions {
        public static IServiceCollection AddMinimalApiStrategies(this IServiceCollection services, Assembly? assembly = null) {
            assembly ??= typeof(IMinimalApiStrategy).Assembly;

            var strategyTypes = assembly
                .GetTypes()
                .Where(type => !type.IsInterface && !type.IsAbstract && typeof(IMinimalApiStrategy).IsAssignableFrom(type));

            foreach (var strategyType in strategyTypes) {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMinimalApiStrategy), strategyType));
            }

            return services;
        }

        public static RouteGroupBuilder MapMinimalApiStrategies(this RouteGroupBuilder group, IEnumerable<IMinimalApiStrategy> strategies) {
            foreach (var strategy in strategies) {
                strategy.MapEndpoints(group);
            }

            return group;
        }
    }
}
