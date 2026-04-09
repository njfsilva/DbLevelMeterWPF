using System.Globalization;

namespace DbLevelMeterWPF.Tests;

public class DbToHeightConverterTests
{
    private readonly DbToHeightConverter _converter = new();

    [Fact]
    public void Convert_NegativeInfinity_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(float.NegativeInfinity, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_MinDb_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-72.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_MaxDb_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert(0.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // MaxHeight is 240
        Assert.InRange((float)result, 239.9f, 240.1f);
    }

    [Fact]
    public void Convert_MidRange_ReturnsProportionalHeight()
    {
        // Arrange
        float midDb = -36.0f; // Midpoint between -72 and 0

        // Act
        object result = _converter.Convert(midDb, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Should be halfway between 0 and 240 = 120
        Assert.InRange((float)result, 119.9f, 120.1f);
    }

    [Fact]
    public void Convert_BelowMinDb_ClampsToMinDb()
    {
        // Act
        object result = _converter.Convert(-100.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Should clamp to -72 and return 0
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_AboveMaxDb_ClampsToMaxDb()
    {
        // Act
        object result = _converter.Convert(10.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Should clamp to 0 and return 240
        Assert.InRange((float)result, 239.9f, 240.1f);
    }

    [Fact]
    public void Convert_Null_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(null, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_NotFloatType_ReturnsZero()
    {
        // Act
        object result = _converter.Convert("not a float", typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_QuarterRange_ReturnsProportionalHeight()
    {
        // Arrange
        float quarterDb = -54.0f; // 25% of range from -72 to 0

        // Act
        object result = _converter.Convert(quarterDb, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Should be 25% of 240 = 60
        Assert.InRange((float)result, 59.9f, 60.1f);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(120.0, typeof(float), null, CultureInfo.InvariantCulture)
        );
    }
}
