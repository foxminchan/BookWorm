using System.Diagnostics.CodeAnalysis;
using BookWorm.Chassis.Exceptions;

namespace BookWorm.Chassis.Utilities.Guards;

public static class GuardAgainstNotFoundExtensions
{
    /// <summary>
    ///     Validates that the provided value is not null. If null, throws a <see cref="NotFoundException" />
    ///     for the specified <paramref name="id" />.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked.</typeparam>
    /// <param name="guard">The guard instance.</param>
    /// <param name="value">The value to check for null.</param>
    /// <param name="id">The identifier associated with the entity being checked.</param>
    /// <exception cref="NotFoundException">
    ///     Thrown when the value is null,
    ///     indicating that the entity with the specified <paramref name="id" /> was not found.
    /// </exception>
    public static void NotFound<T>(this Guard guard, [NotNull] T? value, string id)
    {
        if (value is not null)
        {
            return;
        }

        throw NotFoundException.For<T>(id);
    }

    /// <summary>
    ///     Validates that the provided value is not null. If null, throws a <see cref="NotFoundException" />
    ///     for the specified <paramref name="id" />.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked.</typeparam>
    /// <param name="guard">The guard instance.</param>
    /// <param name="value">The value to check for null.</param>
    /// <param name="id">The identifier associated with the entity being checked.</param>
    /// <exception cref="NotFoundException">
    ///     Thrown when the value is null,
    ///     indicating that the entity with the specified <paramref name="id" /> was not found.
    /// </exception>
    public static void NotFound<T>(this Guard guard, [NotNull] T? value, Guid id)
    {
        if (value is not null)
        {
            return;
        }

        throw NotFoundException.For<T>(id);
    }
}
