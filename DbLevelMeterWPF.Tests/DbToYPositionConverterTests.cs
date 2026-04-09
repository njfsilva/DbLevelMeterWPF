using System.Globalization;

namespace DbLevelMeterWPF.Tests;

public class DbToYPositionConverterTests
{
    private readonly DbToYPositionConverter _converter = new();

    [Fact]
    public void Convert_NegativeInfinity_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert(float.NegativeInfinity, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, 249.9f, 250.1f);
    }

    [Fact]
    public void Convert_MinDb_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert(-72.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, 249.9f, 250.1f);
    }

    [Fact]
    public void Convert_MaxDb_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(0.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_MidRange_ReturnsHalfHeight()
    {
        // Act
        object result = _converter.Convert(-36.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Midpoint between -72 and 0 should give halfway = 125
        Assert.InRange((float)result, 124.9f, 125.1f);
    }

    [Fact]
    public void Convert_BelowMinDb_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert(-100.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, 249.9f, 250.1f);
    }

    [Fact]
    public void Convert_AboveMaxDb_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(10.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_QuarterRange_ReturnsProportionalHeight()
    {
        // Act
        object result = _converter.Convert(-54.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // 25% of range from -72 to 0 should give 75% height = 187.5
        Assert.InRange((float)result, 187.4f, 187.6f);
    }

    [Fact]
    public void Convert_Null_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert(null, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, 249.9f, 250.1f);
    }

    [Fact]
    public void Convert_NotFloatType_ReturnsMaxHeight()
    {
        // Act
        object result = _converter.Convert("not a float", typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, 249.9f, 250.1f);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(125.0, typeof(float), null, CultureInfo.InvariantCulture)
        );
    }
}
