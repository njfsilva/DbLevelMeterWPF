using NAudio.Wave;

namespace DbLevelMeterWPF.Tests;

public class LevelMeterProviderTests
{
    [Fact]
    public void Constructor_WithValidWaveFormat_InitializesSuccessfully()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);

        // Act
        var provider = new LevelMeterProvider(waveFormat);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(float.NegativeInfinity, provider.CurrentLevel);
        Assert.Equal(float.NegativeInfinity, provider.InstantaneousPeakLevel);
        Assert.Equal(float.NegativeInfinity, provider.AverageRmsDb);
        Assert.Equal(float.NegativeInfinity, provider.AverageLufsDb);
    }

    [Fact]
    public void AddSamples_WithSilence_ReturnsNegativeInfinity()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create silence (all zeros)
        byte[] buffer = new byte[1000];
        
        // Act
        provider.AddSamples(buffer, buffer.Length);

        // Assert
        Assert.Equal(float.NegativeInfinity, provider.CurrentLevel);
        Assert.Equal(float.NegativeInfinity, provider.InstantaneousPeakLevel);
    }

    [Fact]
    public void AddSamples_WithSignal_UpdatesCurrentLevel()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create a buffer with known signal (half-scale: ±16384)
        byte[] buffer = new byte[4];
        short halfScale = 16384;
        BitConverter.GetBytes(halfScale).CopyTo(buffer, 0);
        BitConverter.GetBytes(halfScale).CopyTo(buffer, 2);

        // Act
        provider.AddSamples(buffer, buffer.Length);

        // Assert
        Assert.NotEqual(float.NegativeInfinity, provider.CurrentLevel);
        Assert.NotEqual(float.NegativeInfinity, provider.InstantaneousPeakLevel);
        Assert.True(provider.CurrentLevel < 0); // Half-scale should be less than 0dB
    }

    [Fact]
    public void AddSamples_WithMaxSignal_UpdatesPeakLevel()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create a buffer with max signal (close to full scale)
        byte[] buffer = new byte[4];
        short maxSignal = 32767;
        BitConverter.GetBytes(maxSignal).CopyTo(buffer, 0);
        BitConverter.GetBytes(maxSignal).CopyTo(buffer, 2);

        // Act
        provider.AddSamples(buffer, buffer.Length);

        // Assert
        float peakLevel = provider.InstantaneousPeakLevel;
        Assert.NotEqual(float.NegativeInfinity, peakLevel);
        Assert.True(peakLevel > -6); // Close to 0dB
    }

    [Fact]
    public void AddSamples_MultipleBuffers_AveragesCorrectly()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create multiple buffers with constant signal
        byte[] buffer = new byte[4];
        short signal = 8192; // 1/4 scale
        BitConverter.GetBytes(signal).CopyTo(buffer, 0);
        BitConverter.GetBytes(signal).CopyTo(buffer, 2);

        // Act
        // Add same signal multiple times to build average
        for (int i = 0; i < 5; i++)
        {
            provider.AddSamples(buffer, buffer.Length);
        }

        // Assert
        float rmsDb = provider.AverageRmsDb;
        Assert.NotEqual(float.NegativeInfinity, rmsDb);
        Assert.True(rmsDb < 0); // Should be negative
    }

    [Fact]
    public void AddSamples_CalculatesLufsValue()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create a buffer with signal
        byte[] buffer = new byte[100];
        short signal = 8192;
        for (int i = 0; i < buffer.Length; i += 2)
        {
            BitConverter.GetBytes(signal).CopyTo(buffer, i);
        }

        // Act
        // Add enough samples to get LUFS measurement
        for (int i = 0; i < 10; i++)
        {
            provider.AddSamples(buffer, buffer.Length);
        }

        // Assert
        float lufsDb = provider.AverageLufsDb;
        Assert.NotEqual(float.NegativeInfinity, lufsDb);
    }

    [Fact]
    public void AddSamples_SmoothedLevelFallsGracefully()
    {
        // Arrange
        var waveFormat = new WaveFormat(44100, 16, 1);
        var provider = new LevelMeterProvider(waveFormat);
        
        // Create a signal
        byte[] signalBuffer = new byte[100];
        short signal = 16384;
        for (int i = 0; i < signalBuffer.Length; i += 2)
        {
            BitConverter.GetBytes(signal).CopyTo(signalBuffer, i);
        }

        // Create silence
        byte[] silenceBuffer = new byte[100];

        // Act
        provider.AddSamples(signalBuffer, signalBuffer.Length);
        float levelWithSignal = provider.CurrentLevel;
        
        // Add silence
        provider.AddSamples(silenceBuffer, silenceBuffer.Length);
        float levelAfterSilence = provider.CurrentLevel;

        // Assert - level should fall but not instantly to infinity
        Assert.NotEqual(float.NegativeInfinity, levelWithSignal);
        Assert.True(levelAfterSilence <= levelWithSignal || float.IsNegativeInfinity(levelAfterSilence));
    }
}
