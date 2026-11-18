using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.EventBus.Wolverine;

public static class KafkaTopicRouter
{
    public static string CreateTopicName<T>(IHostEnvironment environment)
        where T : IntegrationEvent
    {
        return CreateTopicName(typeof(T), environment.ApplicationName, environment.EnvironmentName);
    }

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

        var topicName = $"{owner.ServiceName.ToKebabCase()}.{owner.TopicName}";
        if (!owner.AppendPrefix)
        {
            return topicName;
        }

        var nameParts = GetApplicationNameParts(applicationName);

        return $"{nameParts[0].ToLowerInvariant()}.{environmentName.ToKebabCase()}.{topicName}";
    }

    public static string CreateDeadLetterTopicName(string applicationName, string environmentName)
    {
        var nameParts = GetApplicationNameParts(applicationName);
        var projectName = nameParts[0];
        var serviceName = string.Join("-", nameParts.Skip(1).Select(ToKebabCase));

        return $"{projectName.ToLowerInvariant()}.{environmentName.ToKebabCase()}.{serviceName.ToKebabCase()}.dead-letter-queue";
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

    extension(string name)
    {
        public string ToKebabCase()
        {
            return string.IsNullOrEmpty(name)
                ? name
                : JsonNamingPolicy.KebabCaseLower.ConvertName(name);
        }
    }
}
