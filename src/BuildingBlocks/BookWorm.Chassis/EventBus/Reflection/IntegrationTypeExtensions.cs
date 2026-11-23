using System.Reflection;
using Wolverine.Attributes;

namespace BookWorm.Chassis.EventBus.Reflection;

internal static class IntegrationTypeExtensions
{
    extension(IEnumerable<Assembly> assemblies)
    {
        internal Type[] ScanIntegrationTypes()
        {
            return
            [
                .. assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type =>
                        type is { IsClass: true, IsAbstract: false }
                        && type.IsIntegrationMessageOrHandler()
                    )
                    .Distinct(),
            ];
        }
    }

    extension(Type type)
    {
        private bool IsIntegrationMessageOrHandler()
        {
            return type.IsAssignableTo(typeof(IntegrationEvent))
                || type.GetMethods(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                    )
                    .Any(method =>
                        method.IsMessageHandlerMethod()
                        && method
                            .GetParameters()[0]
                            .ParameterType.IsAssignableTo(typeof(IntegrationEvent))
                    );
        }
    }

    extension(IEnumerable<Type> types)
    {
        internal IEnumerable<Type> GetHandledIntegrationEventTypes()
        {
            return types
                .Where(t => !t.IsAssignableTo(typeof(IntegrationEvent)))
                .SelectMany(t =>
                    t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                )
                .Where(IsMessageHandlerMethod)
                .Select(m => m.GetParameters()[0].ParameterType)
                .Where(t => t.IsAssignableTo(typeof(IntegrationEvent)));
        }

        internal IEnumerable<Type> GetIntegrationEventTypes()
        {
            return types.Where(t => t.IsAssignableTo(typeof(IntegrationEvent)));
        }
    }

    extension(MethodInfo method)
    {
        private bool IsMessageHandlerMethod()
        {
            return method.GetCustomAttribute<WolverineHandlerAttribute>() is not null
                && method.GetParameters().Length > 0;
        }
    }
}
