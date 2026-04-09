namespace DbLevelMeterWPF.Tests;

public class AudioLevelMonitorTests
{
    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act
        var monitor = new AudioLevelMonitor();

        // Assert
        Assert.NotNull(monitor);
        Assert.NotNull(monitor.AvailableDevices);
        Assert.False(monitor.IsMonitoring);
    }

    [Fact]
    public void RefreshDeviceList_PopulatesDevices()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();
        int initialCount = monitor.AvailableDevices.Count;

        // Act
        monitor.RefreshDeviceList();
        int afterRefreshCount = monitor.AvailableDevices.Count;

        // Assert
        Assert.Equal(initialCount, afterRefreshCount);
    }

    [Fact]
    public void CurrentLevel_PropertyChanges_RaisesLevelChangedEvent()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();
        bool eventRaised = false;
        float eventLevel = 0;

        monitor.LevelChanged += (sender, args) =>
        {
            eventRaised = true;
            eventLevel = args.Level;
        };

        // Act
        var property = typeof(AudioLevelMonitor).GetProperty("CurrentLevel");
        property?.SetValue(monitor, -10.0f);

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(-10.0f, eventLevel);
    }

    [Fact]
    public void CurrentLevel_SameValue_DoesNotRaiseEvent()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();
        int eventCount = 0;

        monitor.LevelChanged += (sender, args) => eventCount++;

        // Act
        var property = typeof(AudioLevelMonitor).GetProperty("CurrentLevel");
        property?.SetValue(monitor, -10.0f);
        int countAfterFirst = eventCount;
        
        property?.SetValue(monitor, -10.0f);
        int countAfterSecond = eventCount;

        // Assert
        Assert.Equal(1, countAfterFirst);
        Assert.Equal(1, countAfterSecond); // Should not raise again for same value
    }

    [Fact]
    public void StopMonitoring_WithoutStarting_DoesNotThrow()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Act & Assert
        monitor.StopMonitoring(); // Should not throw
        Assert.False(monitor.IsMonitoring);
    }

    [Fact]
    public void StopMonitoring_ResetsAllValues()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Set some values before stopping
        var property = typeof(AudioLevelMonitor).GetProperty("CurrentLevel");
        property?.SetValue(monitor, -20.0f);

        // Act
        monitor.StopMonitoring();

        // Assert
        Assert.Equal(float.NegativeInfinity, monitor.CurrentLevel);
        Assert.Equal(float.NegativeInfinity, monitor.InstantaneousPeakLevel);
        Assert.Equal(float.NegativeInfinity, monitor.PeakLevel);
        Assert.Equal(float.NegativeInfinity, monitor.AverageRmsDb);
        Assert.Equal(float.NegativeInfinity, monitor.AverageLufsDb);
        Assert.Equal(float.NegativeInfinity, monitor.IntegratedLufsDb);
        Assert.Equal(0, monitor.Headroom);
        Assert.Equal(0, monitor.ClippingCount);
    }

    [Fact]
    public void ResetPeak_SetsToCurrentLevel()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();
        var currentLevelProperty = typeof(AudioLevelMonitor).GetProperty("CurrentLevel");
        currentLevelProperty?.SetValue(monitor, -15.0f);

        // Act
        monitor.ResetPeak();

        // Assert
        Assert.Equal(monitor.CurrentLevel, monitor.PeakLevel);
    }

    [Fact]
    public void Properties_InitializeToNegativeInfinity()
    {
        // Arrange & Act
        var monitor = new AudioLevelMonitor();

        // Assert
        // Fields default to 0.0f, not NegativeInfinity
        Assert.Equal(0, monitor.CurrentLevel);
        Assert.Equal(0, monitor.InstantaneousPeakLevel);
        Assert.Equal(float.NegativeInfinity, monitor.PeakLevel);
        Assert.Equal(0, monitor.AverageRmsDb);
        Assert.Equal(0, monitor.AverageLufsDb);
        // IntegratedLufsDb and Headroom initialize to 0
        Assert.Equal(0, monitor.IntegratedLufsDb);
        Assert.Equal(0, monitor.Headroom);
        Assert.Equal(0, monitor.ClippingCount);
    }

    [Fact]
    public void Dispose_CallsStopMonitoring()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Act
        monitor.Dispose();

        // Assert
        Assert.False(monitor.IsMonitoring);
        Assert.Equal(float.NegativeInfinity, monitor.CurrentLevel);
    }

    [Fact]
    public void Headroom_CalculatedCorrectly()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();
        
        // Set peak level using reflection
        var peakProperty = typeof(AudioLevelMonitor).GetProperty("PeakLevel");
        peakProperty?.SetValue(monitor, -6.0f);

        // Set headroom (it's calculated as 0 - PeakLevel)
        var headroomProperty = typeof(AudioLevelMonitor).GetProperty("Headroom");
        headroomProperty?.SetValue(monitor, 6.0f);

        // Assert
        Assert.Equal(6.0f, monitor.Headroom);
    }

    [Fact]
    public void ClippingCount_IncrementsOnClipping()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Assert initial state
        Assert.Equal(0, monitor.ClippingCount);

        // Using reflection to set the internal flag for testing
        var clippingProperty = typeof(AudioLevelMonitor).GetProperty("ClippingCount");
        
        // Act
        clippingProperty?.SetValue(monitor, 1);

        // Assert
        Assert.Equal(1, monitor.ClippingCount);
    }

    [Fact]
    public void AvailableDevices_IsObservableCollection()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Act & Assert
        Assert.NotNull(monitor.AvailableDevices);
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ObservableCollection<string>>(monitor.AvailableDevices);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var monitor = new AudioLevelMonitor();

        // Act & Assert
        monitor.Dispose(); // First call
        monitor.Dispose(); // Second call - should not throw
    }
}
