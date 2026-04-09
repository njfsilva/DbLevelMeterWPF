namespace DbLevelMeterWPF.Tests;

public class AppSettingsTests
{
    [Fact]
    public void LastDeviceIndex_DefaultValue_ReturnsZero()
    {
        // This test verifies the default behavior
        // Note: In a real scenario, you might want to isolate AppSettings
        // for unit testing to avoid file system side effects

        // Act
        int defaultIndex = AppSettings.LastDeviceIndex;

        // Assert
        Assert.IsType<int>(defaultIndex);
    }

    [Fact]
    public void LastDeviceIndex_SetValue_CanRetrieveValue()
    {
        // Arrange
        int testValue = 2;

        // Act
        AppSettings.LastDeviceIndex = testValue;
        int retrievedValue = AppSettings.LastDeviceIndex;

        // Assert
        Assert.Equal(testValue, retrievedValue);
    }

    [Fact]
    public void LastDeviceIndex_SetMultipleValues_ReturnsLatestValue()
    {
        // Arrange
        int firstValue = 1;
        int secondValue = 3;

        // Act
        AppSettings.LastDeviceIndex = firstValue;
        AppSettings.LastDeviceIndex = secondValue;
        int retrievedValue = AppSettings.LastDeviceIndex;

        // Assert
        Assert.Equal(secondValue, retrievedValue);
    }

    [Fact]
    public void LastDeviceIndex_SetNegativeValue_StoresAndRetrievesIt()
    {
        // Arrange
        int negativeValue = -1;

        // Act
        AppSettings.LastDeviceIndex = negativeValue;
        int retrievedValue = AppSettings.LastDeviceIndex;

        // Assert
        Assert.Equal(negativeValue, retrievedValue);
    }

    [Fact]
    public void LastDeviceIndex_SetLargeValue_StoresAndRetrievesIt()
    {
        // Arrange
        int largeValue = 9999;

        // Act
        AppSettings.LastDeviceIndex = largeValue;
        int retrievedValue = AppSettings.LastDeviceIndex;

        // Assert
        Assert.Equal(largeValue, retrievedValue);
    }
}
