using System.Globalization;

namespace DbLevelMeterWPF.Tests;

public class GreenLevelHeightConverterTests
{
    private readonly GreenLevelHeightConverter _converter = new();

    [Fact]
    public void Convert_NegativeInfinity_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(float.NegativeInfinity, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_BelowGreenRange_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-100.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Convert_AtGreenMin_ReturnsZero()
    {
        // Act
        object result = _converter.Convert(-72.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.InRange((float)result, -0.01f, 0.01f);
    }

    [Fact]
    public void Convert_AtGreenMax_ReturnsMaxGreenHeight()
    {
        // Act
        object result = _converter.Convert(-18.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // MaxHeight / 3 = 80
        Assert.InRange((float)result, 79.9f, 80.1f);
    }

    [Fact]
    public void Convert_AtGreenMidpoint_ReturnsHalfGreenHeight()
    {
        // Act
        object result = _converter.Convert(-45.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
        // Midpoint between -72 and -18 should give half of 80 = 40
        Assert.InRange((float)result, 39.9f, 40.1f);
    }

    [Fact]
    public void Convert_AboveGreenMax_ReturnsMaxGreenHeight()
    {
        // Act
        object result = _converter.Convert(0.0f, typeof(double), null, CultureInfo.InvariantCulture);

        // Assert
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
