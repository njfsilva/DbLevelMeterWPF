using System.Globalization;

namespace DbLevelMeterWPF.Tests;

public class YellowLevelHeightConverterTests
{
    private readonly YellowLevelHeightConverter _converter = new();

    [Fact]
    public void Convert_NegativeInfinity_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(float.NegativeInfinity, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_BelowYellowRange_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-50.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_AtYellowMin_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-18.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_AtYellowMax_ReturnsFullHeight()
    {
        // Act
        object result = _converter.Convert(-10.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // At YellowMax (-10), level is at the boundary, should show full height
        Assert.InRange((float)result, 79.9f, 80.1f);
    }

    [Fact]
    public void Convert_WithinYellowRange_ReturnsPartialHeight()
    {
        // Act
        object result = _converter.Convert(-14.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Midpoint between -18 and -10 should give half of 80 = 40
        Assert.InRange((float)result, 39.9f, 40.1f);
    }

    [Fact]
    public void Convert_AtYellowMidpoint_ReturnsHalfYellowHeight()
    {
        // Act
        object result = _converter.Convert(-14.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // (-14 - (-18)) / (-10 - (-18)) = 4/8 = 0.5, so 0.5 * 80 = 40
        Assert.InRange((float)result, 39.9f, 40.1f);
    }

    [Fact]
    public void Convert_AboveYellowRange_ReturnsFullHeight()
    {
        // Act
        object result = _converter.Convert(-5.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Above yellow range should be in red range, but yellow still shows full to be stacked
        Assert.InRange((float)result, 79.9f, 80.1f);
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
