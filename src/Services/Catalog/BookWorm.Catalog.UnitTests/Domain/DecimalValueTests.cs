using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class DecimalValueTests
{
    [Test]
    public void GivenValidUnitsAndNanos_WhenCreatingDecimalValue_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const long expectedUnits = 123;
        const int expectedNanos = 456_789_000;

        // Act
        var decimalValue = new DecimalValue(expectedUnits, expectedNanos);

        // Assert
        decimalValue.Units.ShouldBe(expectedUnits);
        decimalValue.Nanos.ShouldBe(expectedNanos);
    }

    [Test]
    public void GivenNegativeUnits_WhenCreatingDecimalValue_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const long expectedUnits = -123;
        const int expectedNanos = 456_789_000;

        // Act
        var decimalValue = new DecimalValue(expectedUnits, expectedNanos);

        // Assert
        decimalValue.Units.ShouldBe(expectedUnits);
        decimalValue.Nanos.ShouldBe(expectedNanos);
    }

    [Test]
    public void GivenZeroValues_WhenCreatingDecimalValue_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const long expectedUnits = 0;
        const int expectedNanos = 0;

        // Act
        var decimalValue = new DecimalValue(expectedUnits, expectedNanos);

        // Assert
        decimalValue.Units.ShouldBe(expectedUnits);
        decimalValue.Nanos.ShouldBe(expectedNanos);
    }

    [Test]
    public void GivenMaxValues_WhenCreatingDecimalValue_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const long expectedUnits = long.MaxValue;
        const int expectedNanos = int.MaxValue;

        // Act
        var decimalValue = new DecimalValue(expectedUnits, expectedNanos);

        // Assert
        decimalValue.Units.ShouldBe(expectedUnits);
        decimalValue.Nanos.ShouldBe(expectedNanos);
    }

    [Test]
    public void GivenMinValues_WhenCreatingDecimalValue_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const long expectedUnits = long.MinValue;
        const int expectedNanos = int.MinValue;

        // Act
        var decimalValue = new DecimalValue(expectedUnits, expectedNanos);

        // Assert
        decimalValue.Units.ShouldBe(expectedUnits);
        decimalValue.Nanos.ShouldBe(expectedNanos);
    }

    [Test]
    public void GivenNullDecimalValue_WhenConvertingToDecimal_ThenShouldReturnNull()
    {
        // Arrange
        DecimalValue? nullDecimalValue = null;

        // Act
        decimal? result = nullDecimalValue;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GivenValidDecimalValue_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(123, 456_789_000);
        const decimal expected = 123.456789m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenZeroDecimalValue_WhenConvertingToDecimal_ThenShouldReturnZero()
    {
        // Arrange
        var decimalValue = new DecimalValue(0, 0);
        const decimal expected = 0m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenNegativeUnitsPositiveNanos_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(-123, 456_789_000);
        const decimal expected = -122.543211m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenPositiveUnitsNegativeNanos_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(123, -456_789_000);
        const decimal expected = 122.543211m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenDecimalValueWithOnlyUnits_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(123, 0);
        const decimal expected = 123m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenDecimalValueWithOnlyNanos_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(0, 123_456_789);
        const decimal expected = 0.123456789m;

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenNullDecimal_WhenConvertingToDecimalValue_ThenShouldReturnNull()
    {
        // Arrange
        decimal? nullDecimal = null;

        // Act
        DecimalValue? result = nullDecimal;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GivenValidDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 123.456789m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(123);
        result.Nanos.ShouldBe(456_789_000);
    }

    [Test]
    public void GivenZeroDecimal_WhenConvertingToDecimalValue_ThenShouldReturnZeroDecimalValue()
    {
        // Arrange
        const decimal value = 0m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(0);
        result.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenNegativeDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = -123.456789m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(-123);
        result.Nanos.ShouldBe(-456_789_000);
    }

    [Test]
    public void GivenWholeNumberDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 123m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(123);
        result.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenFractionalOnlyDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 0.123456789m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(0);
        result.Nanos.ShouldBe(123_456_789);
    }

    [Test]
    public void GivenVerySmallDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 0.000000001m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(0);
        result.Nanos.ShouldBe(1);
    }

    [Test]
    public void GivenLargeDecimal_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 999_999_999.999999999m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(999_999_999);
        result.Nanos.ShouldBe(999_999_999);
    }

    [Test]
    public void GivenDecimalWithMoreThanNinePrecision_WhenConvertingToDecimalValue_ThenShouldTruncatePrecision()
    {
        // Arrange
        const decimal value = 123.1234567891m; // 10 decimal places

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(123);
        result.Nanos.ShouldBe(123_456_789); // Truncated to 9 decimal places
    }

    [Test]
    public void GivenRoundTripConversion_WhenConvertingDecimalToDecimalValueAndBack_ThenShouldPreserveValue()
    {
        // Arrange
        const decimal originalValue = 123.456789m;

        // Act
        DecimalValue? decimalValue = originalValue;
        decimal? convertedBack = decimalValue;

        // Assert
        convertedBack.ShouldBe(originalValue);
    }

    [Test]
    public void GivenRoundTripConversionWithNegative_WhenConvertingDecimalToDecimalValueAndBack_ThenShouldPreserveValue()
    {
        // Arrange
        const decimal originalValue = -123.456789m;

        // Act
        DecimalValue? decimalValue = originalValue;
        decimal? convertedBack = decimalValue;

        // Assert
        convertedBack.ShouldBe(originalValue);
    }

    [Test]
    public void GivenRoundTripConversionWithZero_WhenConvertingDecimalToDecimalValueAndBack_ThenShouldPreserveValue()
    {
        // Arrange
        const decimal originalValue = 0m;

        // Act
        DecimalValue? decimalValue = originalValue;
        decimal? convertedBack = decimalValue;

        // Assert
        convertedBack.ShouldBe(originalValue);
    }

    [Test]
    public void GivenMaxDecimalValue_WhenConvertingToDecimal_ThenShouldHandleCorrectly()
    {
        // Arrange
        var decimalValue = new DecimalValue(long.MaxValue, int.MaxValue);

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(long.MaxValue + (int.MaxValue / 1_000_000_000m));
    }

    [Test]
    public void GivenMinDecimalValue_WhenConvertingToDecimal_ThenShouldHandleCorrectly()
    {
        // Arrange
        var decimalValue = new DecimalValue(long.MinValue, int.MinValue);

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(long.MinValue + (int.MinValue / 1_000_000_000m));
    }

    [Test]
    public void GivenDecimalValueWithExactNanoFactor_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(1, 1_000_000_000);
        const decimal expected = 2m; // 1 + (1_000_000_000 / 1_000_000_000)

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenDecimalEqualToNanoFactor_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 1_000_000_000m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(1_000_000_000);
        result.Nanos.ShouldBe(0);
    }

    [Test]
    public void GivenNanoFactorPlusOne_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 1_000_000_000.000000001m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(1_000_000_000);
        result.Nanos.ShouldBe(1);
    }

    [Test]
    public void GivenExactlyOneNanoPrecision_WhenConvertingBothWays_ThenShouldMaintainPrecision()
    {
        // Arrange
        const decimal originalValue = 0.000000001m; // Exactly 1 nano

        // Act
        DecimalValue? decimalValue = originalValue;
        decimal? convertedBack = decimalValue;

        // Assert
        convertedBack.ShouldBe(originalValue);
    }

    [Test]
    public void GivenNegativeNanoFactor_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(1, -1_000_000_000);
        const decimal expected = 0m; // 1 + (-1_000_000_000 / 1_000_000_000) = 1 - 1 = 0

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenLargeNegativeNanos_WhenConvertingToDecimal_ThenShouldReturnCorrectValue()
    {
        // Arrange
        var decimalValue = new DecimalValue(5, -2_000_000_000);
        const decimal expected = 3m; // 5 + (-2_000_000_000 / 1_000_000_000) = 5 - 2 = 3

        // Act
        decimal? result = decimalValue;

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public void GivenSubNanoPrecisionDecimal_WhenConvertingToDecimalValue_ThenShouldTruncateToNano()
    {
        // Arrange
        const decimal value = 0.0000000005m;

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(0);
        result.Nanos.ShouldBe(0); // Truncated because it's less than 1 nano
    }

    [Test]
    public void GivenDecimalWithExactNanoFactorDivision_WhenConvertingToDecimalValue_ThenShouldReturnCorrectValue()
    {
        // Arrange
        const decimal value = 999.999999999m; // 9 nines after decimal

        // Act
        DecimalValue? result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Units.ShouldBe(999);
        result.Nanos.ShouldBe(999_999_999);
    }
}
