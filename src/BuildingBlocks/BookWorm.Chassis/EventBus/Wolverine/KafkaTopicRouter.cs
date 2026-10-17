using System.Reflection;

namespace BookWorm.Chassis.EventBus.Wolverine;

internal static class KafkaTopicRouter
{
    public static string CreateTopicName(
        Type messageType,
        string applicationName,
        string environmentName
    )
    {
        var owner = messageType.GetCustomAttribute<IntegrationMessageOwnerAttribute>();
        if (owner is null || string.IsNullOrWhiteSpace(owner.ServiceName))
        {
            throw new InvalidOperationException(
                $"Integration message '{messageType.FullName}' must declare an "
                    + $"{nameof(IntegrationMessageOwnerAttribute)}."
            );
        }

        var nameParts = GetApplicationNameParts(applicationName);
        var projectName = nameParts[0];
        var eventName = GetEventName(messageType, owner.ServiceName);

        return string.Join(
            ".",
            projectName.ToLowerInvariant(),
            ToKebabCase(environmentName),
            ToKebabCase(owner.ServiceName),
            eventName
        );
    }

    public static string CreateDeadLetterTopicName(string applicationName, string environmentName)
    {
        var nameParts = GetApplicationNameParts(applicationName);
        var projectName = nameParts[0];
        var serviceName = string.Join("-", nameParts.Skip(1).Select(ToKebabCase));

        return string.Join(
            ".",
            projectName.ToLowerInvariant(),
            ToKebabCase(environmentName),
            ToKebabCase(serviceName),
            "dead-letter-queue"
        );
    }

    private static string[] GetApplicationNameParts(string applicationName)
    {
        var nameParts = applicationName.Split('.');
        if (nameParts.Length < 2 || nameParts.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                $"The host application name '{applicationName}' must contain a project and service name."
            );
        }

        return nameParts;
    }

    private static string GetEventName(Type messageType, string serviceName)
    {
        var typeName = messageType.Name;
        foreach (var suffix in new[] { "IntegrationEvent", "Command" })
        {
            if (!typeName.EndsWith(suffix, StringComparison.Ordinal))
            {
                continue;
            }

            typeName = typeName[..^suffix.Length];
            break;
        }

        if (typeName.StartsWith(serviceName, StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName[serviceName.Length..];
        }

        var eventName = ToKebabCase(typeName)
            .Replace("deleted", "delete", StringComparison.Ordinal)
            .Replace("created", "create", StringComparison.Ordinal)
            .Replace("updated", "update", StringComparison.Ordinal)
            .Replace("changed", "change", StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new InvalidOperationException(
                $"Integration message '{messageType.FullName}' does not contain an event name."
            );
        }

        return eventName;
    }

    public static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return string.Concat(
            name.Select(
                (c, i) =>
                    i > 0 && char.IsUpper(c)
                        ? "-" + char.ToLowerInvariant(c)
                        : char.ToLowerInvariant(c).ToString()
            )
        );
    }
}
