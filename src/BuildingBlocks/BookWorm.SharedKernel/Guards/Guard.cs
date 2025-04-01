namespace BookWorm.SharedKernel.Guards;

public sealed class Guard
{
    private Guard() { }

    public static Guard Against { get; } = new();
}
