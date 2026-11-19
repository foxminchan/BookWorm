using System.Reflection;
using System.Text.RegularExpressions;
using Wolverine.Attributes;

namespace BookWorm.Testing.Pact.Messaging;

internal static partial class PactMessageConventions
{
    public static string GetMessageType(Type messageType)
    {
        return messageType.GetCustomAttribute<MessageIdentityAttribute>()?.Alias
            ?? messageType.FullName
            ?? messageType.Name;
    }

    public static string GetMessageDescription(string messageType)
    {
        return $"{messageType} message";
    }

    public static string GetMessageSource(string provider)
    {
        var kebabCase = MessageSourceRegex()
            .Replace($"BookWorm.{provider}".Replace('.', '-'), "$1-$2");

        return $"urn:bookworm:{kebabCase.ToLowerInvariant()}";
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex MessageSourceRegex();
}
