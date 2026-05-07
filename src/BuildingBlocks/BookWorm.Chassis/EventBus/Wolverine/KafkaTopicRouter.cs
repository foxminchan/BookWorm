namespace BookWorm.Chassis.EventBus.Wolverine;

internal static class KafkaTopicRouter
{
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
