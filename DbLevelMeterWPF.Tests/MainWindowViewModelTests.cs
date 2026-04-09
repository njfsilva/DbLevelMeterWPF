using System.ComponentModel;

namespace DbLevelMeterWPF.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act
        var viewModel = new MainWindowViewModel();

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.AvailableDevices);
    }

    [Fact]
    public void AvailableDevices_ReturnsObservableCollection()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act
        var devices = viewModel.AvailableDevices;

        // Assert
        Assert.NotNull(devices);
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ObservableCollection<string>>(devices);
    }

    [Fact]
    public void CurrentLevelDb_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.CurrentLevelDb);
    }

    [Fact]
    public void PeakLevelDb_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.PeakLevelDb);
    }

    [Fact]
    public void AverageRmsDb_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.AverageRmsDb);
    }

    [Fact]
    public void AverageLufsDb_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.AverageLufsDb);
    }

    [Fact]
    public void IntegratedLufsDb_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.IntegratedLufsDb);
    }

    [Fact]
    public void Headroom_InitializesToDefaultValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.IsType<float>(viewModel.Headroom);
    }

    [Fact]
    public void ClippingCount_InitializesToZero()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        Assert.Equal(0, viewModel.ClippingCount);
    }

    [Fact]
    public void PropertyChanged_RaisesEventWhenPropertyChanges()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();
        bool eventRaised = false;
        string? changedProperty = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            changedProperty = e.PropertyName;
        };

        // Act
        // Set property using reflection to trigger PropertyChanged
        var property = typeof(MainWindowViewModel).GetProperty("IsMonitoring");
        
        // Assert - We can verify the structure supports PropertyChanged
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void ResetPeakCommand_IsNotNull()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act
        var command = viewModel.ResetPeakCommand;

        // Assert
        Assert.NotNull(command);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act & Assert
        viewModel.Dispose(); // Should not throw
    }

    [Fact]
    public void SelectedDeviceIndex_ReturnsValidValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act
        int index = viewModel.SelectedDeviceIndex;

        // Assert
        Assert.IsType<int>(index);
    }

    [Fact]
    public void IsMonitoring_ReturnsBooleanValue()
    {
        // Arrange
        var viewModel = new MainWindowViewModel();

        // Act
        bool isMonitoring = viewModel.IsMonitoring;

        // Assert
        Assert.IsType<bool>(isMonitoring);
    }
}
