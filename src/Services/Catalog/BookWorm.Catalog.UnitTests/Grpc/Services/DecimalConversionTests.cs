using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Catalog.UnitTests.Grpc.Services;

public sealed class DecimalConversionTests
{
    [Test]
    public void GivenPositiveDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainPrecision()
    {
        // Arrange
        var originalValue = 123.456789m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(123);
        protoDecimal.Nanos.ShouldBe(456789000); // 456789000 nanoseconds = 0.456789 seconds
    }

    [Test]
    public void GivenNegativeDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainPrecision()
    {
        // Arrange
        var originalValue = -98.123456789m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(-99); // Floor division: -99 + 0.876543211 = -98.123456789
        protoDecimal.Nanos.ShouldBe(876543211); // Nanos is always positive (fractional part)
    }

    [Test]
    public void GivenZeroDecimal_WhenConvertingToAndFromDecimal_ThenShouldReturnZero()
    {
        // Arrange
        var originalValue = 0m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(0);
        protoDecimal.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenWholeNumberDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainValue()
    {
        // Arrange
        var originalValue = 42m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(42);
        protoDecimal.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenHighPrecisionDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainPrecision()
    {
        // Arrange - Use a value with 9 decimal places (maximum precision for nanoseconds)
        var originalValue = 1.123456789m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(1);
        protoDecimal.Nanos.ShouldBe(123456789);
    }

    [Test]
    public void GivenNullDecimal_WhenConvertingFromDecimal_ThenShouldReturnZero()
    {
        // Arrange
        Decimal? nullDecimal = null;

        // Act
        var result = BookService.FromDecimal(nullDecimal!);

        // Assert
        result.ShouldBe(0m);
    }

    [Test]
    public void GivenRoundingEdgeCase_WhenConvertingToDecimal_ThenShouldHandleCorrectly()
    {
        // Arrange - Use a value that would cause rounding issues
        var originalValue = 0.9999999999m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        // Due to nanosecond precision (9 decimal places), values beyond that get rounded
        convertedBack.ShouldBe(1m); // Should round to 1
        protoDecimal.Units.ShouldBe(1);
        protoDecimal.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenVerySmallDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainPrecision()
    {
        // Arrange
        var originalValue = 0.000000001m; // 1 nanosecond

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(0);
        protoDecimal.Nanos.ShouldBe(1);
    }

    [Test]
    public void GivenValueTooLarge_WhenConvertingToDecimal_ThenShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var tooLargeValue = (decimal)long.MaxValue + 1m;

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => BookService.ToDecimal(tooLargeValue));
    }

    [Test]
    public void GivenValueTooSmall_WhenConvertingToDecimal_ThenShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var tooSmallValue = (decimal)long.MinValue - 1m;

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => BookService.ToDecimal(tooSmallValue));
    }

    [Test]
    public void GivenNegativeSmallDecimal_WhenConvertingToAndFromDecimal_ThenShouldMaintainPrecision()
    {
        // Arrange
        var originalValue = -0.123456789m;

        // Act
        var protoDecimal = BookService.ToDecimal(originalValue);
        var convertedBack = BookService.FromDecimal(protoDecimal);

        // Assert
        convertedBack.ShouldBe(originalValue);
        protoDecimal.Units.ShouldBe(-1); // Floor division: -1 + 0.876543211 = -0.123456789
        protoDecimal.Nanos.ShouldBe(876543211); // Fractional part is positive
    }
}