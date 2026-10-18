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
        if (
            owner is null
            || string.IsNullOrWhiteSpace(owner.ServiceName)
            || string.IsNullOrWhiteSpace(owner.TopicName)
        )
        {
            throw new InvalidOperationException(
                $"Integration message '{messageType.FullName}' must declare non-empty "
                    + $"{nameof(IntegrationMessageOwnerAttribute.ServiceName)} and "
                    + $"{nameof(IntegrationMessageOwnerAttribute.TopicName)}."
            );
        }

        var topicName = string.Join(".", ToKebabCase(owner.ServiceName), owner.TopicName);
        if (!owner.AppendPrefix)
        {
            return topicName;
        }

        var nameParts = GetApplicationNameParts(applicationName);

        return string.Join(
            ".",
            nameParts[0].ToLowerInvariant(),
            ToKebabCase(environmentName),
            topicName
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
