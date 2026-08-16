namespace BookWorm.Chat.Configurations;

internal sealed class LoopEngineeringOptions
{
    public const string ConfigurationSection = "Chat:LoopEngineering";

    public int MaxIterations { get; set; } = 3;

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(2);
}
