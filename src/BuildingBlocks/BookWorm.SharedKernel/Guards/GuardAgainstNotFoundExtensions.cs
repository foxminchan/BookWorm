using System.Diagnostics.CodeAnalysis;
using BookWorm.SharedKernel.Exceptions;

namespace BookWorm.SharedKernel.Guards;

public static class GuardAgainstNotFoundExtensions
{
    /// <summary>
    ///     Validates that the provided value is not null. If null, throws a <see cref="NotFoundException" />.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked.</typeparam>
    /// <param name="guard">The guard instance.</param>
    /// <param name="value">The value to check for null.</param>
    /// <param name="message">Optional custom error message. If not provided, a default message will be created.</param>
    /// <exception cref="NotFoundException">Thrown when the value is null.</exception>
    public static void NotFound<T>(this Guard guard, [NotNull] T? value, string? message = null)
    {
        if (value is not null)
        {
            return;
        }

        message ??= $"The {typeof(T).Name} was not found.";
        throw new NotFoundException(message);
    }
}
