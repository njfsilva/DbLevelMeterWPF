using System.Globalization;

namespace DbLevelMeterWPF.Tests;

public class RedLevelHeightConverterTests
{
    private readonly RedLevelHeightConverter _converter = new();

    [Fact]
    public void Convert_NegativeInfinity_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(float.NegativeInfinity, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_BelowRedRange_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-50.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_AtRedMin_ReturnsPartialHeight()
    {
        // Act
        object result = _converter.Convert(-10.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // At min of red range, should show minimal red
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_AtRedMax_ReturnsFullHeight()
    {
        // Act
        object result = _converter.Convert(0.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // At RedMax (0), clipping detected, should show full height
        Assert.InRange((float)result, 79.9f, 80.1f);
    }

    [Fact]
    public void Convert_AtRedMidpoint_ReturnsHalfRedHeight()
    {
        // Act
        object result = _converter.Convert(-5.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Midpoint between -10 and 0 should give half of 80 = 40
        Assert.InRange((float)result, 39.9f, 40.1f);
    }

    [Fact]
    public void Convert_JustBelowRedMax_ReturnsMaxRedHeight()
    {
        // Act
        object result = _converter.Convert(-0.5f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // -0.5 is 5% of red range (-10 to 0), so should be ~76 (0.95 * 80)
        Assert.InRange((float)result, 75.0f, 77.0f);
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
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(40.0, typeof(float), null, CultureInfo.InvariantCulture)
        );
    }
}
