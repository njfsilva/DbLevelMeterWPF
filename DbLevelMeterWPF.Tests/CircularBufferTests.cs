namespace DbLevelMeterWPF.Tests;

public class CircularBufferTests
{
    [Fact]
    public void Constructor_CreatesBuffer_WithCorrectSize()
    {
        // Arrange
        int size = 1000;

        // Act
        var buffer = new CircularBuffer(size);

        // Assert
        Assert.NotNull(buffer);
    }

    [Fact]
    public void AddSamples_SingleSample_CanRetrieveSample()
    {
        // Arrange
        var buffer = new CircularBuffer(100);
        float[] samples = [1.5f];

        // Act
        buffer.AddSamples(samples);
        float[] result = buffer.GetAllSamples();

        // Assert
        Assert.Single(result);
        Assert.Equal(1.5f, result[0]);
    }

    [Fact]
    public void AddSamples_MultipleSamples_CanRetrieveAll()
    {
        // Arrange
        var buffer = new CircularBuffer(1000);
        float[] samples = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f];

        // Act
        buffer.AddSamples(samples);
        float[] result = buffer.GetAllSamples();

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(samples, result);
    }

    [Fact]
    public void AddSamples_BufferFull_OverwritesOldestData()
    {
        // Arrange
        int bufferSize = 5;
        var buffer = new CircularBuffer(bufferSize);
        float[] firstBatch = [1.0f, 2.0f, 3.0f];
        float[] secondBatch = [4.0f, 5.0f, 6.0f];

        // Act
        buffer.AddSamples(firstBatch);
        buffer.AddSamples(secondBatch);
        float[] result = buffer.GetAllSamples();

        // Assert
        Assert.Equal(bufferSize, result.Length);
        // Buffer should contain [2.0f, 3.0f, 4.0f, 5.0f, 6.0f]
        Assert.Equal([2.0f, 3.0f, 4.0f, 5.0f, 6.0f], result);
    }

    [Fact]
    public void GetAllSamples_EmptyBuffer_ReturnsEmptyArray()
    {
        // Arrange
        var buffer = new CircularBuffer(100);

        // Act
        float[] result = buffer.GetAllSamples();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AddSamples_PartiallyFilledBuffer_ReturnsOnlyAddedSamples()
    {
        // Arrange
        var buffer = new CircularBuffer(1000);
        float[] samples = [1.0f, 2.0f];

        // Act
        buffer.AddSamples(samples);
        float[] result = buffer.GetAllSamples();

        // Assert
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void AddSamples_WrapAround_ReturnsCorrectOrder()
    {
        // Arrange
        int bufferSize = 5;
        var buffer = new CircularBuffer(bufferSize);
        
        // Fill buffer to capacity
        float[] batch1 = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f];
        buffer.AddSamples(batch1);

        // Add one more sample to cause wrap-around
        float[] batch2 = [6.0f];
        buffer.AddSamples(batch2);

        // Act
        float[] result = buffer.GetAllSamples();

        // Assert - should be [2.0f, 3.0f, 4.0f, 5.0f, 6.0f] (oldest data overwritten)
        Assert.Equal(5, result.Length);
        Assert.Equal([2.0f, 3.0f, 4.0f, 5.0f, 6.0f], result);
    }

    [Fact]
    public void AddSamples_ConcurrentAccess_MaintainsConsistency()
    {
        // Arrange
        var buffer = new CircularBuffer(1000);
        int taskCount = 5;
        int samplesPerTask = 100;

        // Act
        var tasks = new Task[taskCount];
        for (int t = 0; t < taskCount; t++)
        {
            int taskIndex = t;
            tasks[t] = Task.Run(() =>
            {
                float[] samples = new float[samplesPerTask];
                for (int i = 0; i < samplesPerTask; i++)
                {
                    samples[i] = taskIndex * samplesPerTask + i;
                }
                buffer.AddSamples(samples);
            });
        }
        Task.WaitAll(tasks);
        float[] result = buffer.GetAllSamples();

        // Assert - should have 500 samples total
        Assert.Equal(500, result.Length);
    }
}
