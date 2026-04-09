using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF.Tests;

public class BoolInverterConverterTests
{
    private readonly BoolInverterConverter _converter = new();

    [Fact]
    public void Convert_TrueToFalse()
    {
        // Arrange
        bool input = true;

        // Act
        object result = _converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void Convert_FalseToTrue()
    {
        // Arrange
        bool input = false;

        // Act
        object result = _converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void Convert_NullToFalse()
    {
        // Act
        object result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void Convert_NonBooleanToFalse()
    {
        // Act
        object result = _converter.Convert("not a bool", typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void ConvertBack_TrueToFalse()
    {
        // Arrange
        bool input = true;

        // Act
        object result = _converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result);
    }

    [Fact]
    public void ConvertBack_FalseToTrue()
    {
        // Arrange
        bool input = false;

        // Act
        object result = _converter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertBack_NullToFalse()
    {
        // Act
        object result = _converter.ConvertBack(null, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.False((bool)result);
    }
}
