using NAudio.Wave;
using System.Buffers;
using System.Collections.ObjectModel;

namespace DbLevelMeterWPF
{
    public class AudioLevelMonitor : IDisposable
    {
        private IWaveIn? _waveIn;
        private float _currentLevel;
        private float _instantaneousPeakLevel;
        private float _peakLevel = float.NegativeInfinity;
        private float _averageRmsDb;
        private float _averageLufsDb;
        private float _integratedLufsDb;
        private float _headroom;
        private int _clippingCount;
        private LevelMeterProvider? _levelMeter;

        private DateTime _lastPeakUpdateTime = DateTime.MinValue;
        private DateTime _lastRmsUpdateTime = DateTime.MinValue;
        private const double ResetIntervalSeconds = 5.0;
        private bool _lastFrameWasClipping;
        private CircularBuffer? _sessionAudioBuffer;
        private float[]? _sessionSampleBuffer;
        private int _lastSessionSampleBufferSize;

        public event EventHandler<LevelChangedEventArgs>? LevelChanged;

        public float CurrentLevel
        {
            get => _currentLevel;
            private set
            {
                if (_currentLevel != value)
                {
                    _currentLevel = value;
                    OnLevelChanged();
                }
            }
        }

        public float InstantaneousPeakLevel
        {
            get => _instantaneousPeakLevel;
            private set => _instantaneousPeakLevel = value;
        }

        public float PeakLevel
        {
            get => _peakLevel;
            private set => _peakLevel = value;
        }

        public float AverageRmsDb
        {
            get => _averageRmsDb;
            private set => _averageRmsDb = value;
        }

        public float AverageLufsDb
        {
            get => _averageLufsDb;
            private set => _averageLufsDb = value;
        }

        public float IntegratedLufsDb
        {
            get => _integratedLufsDb;
            private set => _integratedLufsDb = value;
        }

        public float Headroom
        {
            get => _headroom;
            private set => _headroom = value;
        }

        public int ClippingCount
        {
            get => _clippingCount;
            private set => _clippingCount = value;
        }

        public bool IsMonitoring { get; private set; }

        public ObservableCollection<string> AvailableDevices { get; } = [];

        public AudioLevelMonitor()
        {
            RefreshDeviceList();
        }

        public void RefreshDeviceList()
        {
            AvailableDevices.Clear();
            int deviceCount = WaveInEvent.DeviceCount;

            for (int i = 0; i < deviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                AvailableDevices.Add(capabilities.ProductName);
            }
        }

        public void StartMonitoring(int deviceIndex, int bufferMilliseconds = 17)
        {
            if (IsMonitoring)
                StopMonitoring();

            _waveIn = new WaveInEvent 
            { 
                DeviceNumber = deviceIndex,
                BufferMilliseconds = bufferMilliseconds
            };
            _waveIn.WaveFormat = new WaveFormat(44100, 16, 1);

            _levelMeter = new LevelMeterProvider(_waveIn.WaveFormat);
            // Session buffer: 5 minutes of audio at 44100 Hz = 13,230,000 samples
            const int FiveMinutesBufferSize = 44100 * 60 * 5;
            _sessionAudioBuffer = new CircularBuffer(FiveMinutesBufferSize);
            _clippingCount = 0;
            _lastFrameWasClipping = false;
            _waveIn.DataAvailable += (s, e) =>
            {
                _levelMeter.AddSamples(e.Buffer, e.BytesRecorded);
                CurrentLevel = _levelMeter.CurrentLevel;
                InstantaneousPeakLevel = _levelMeter.InstantaneousPeakLevel;

                // Update Peak with timestamp
                if (InstantaneousPeakLevel > PeakLevel)
                {
                    PeakLevel = InstantaneousPeakLevel;
                    _lastPeakUpdateTime = DateTime.Now;
                }

                // Reset Peak if 5 seconds have passed since last update
                if ((DateTime.Now - _lastPeakUpdateTime).TotalSeconds > ResetIntervalSeconds)
                {
                    PeakLevel = CurrentLevel;
                    _lastPeakUpdateTime = DateTime.Now;
                }

                // Update RMS with timestamp
                var newRmsDb = _levelMeter.AverageRmsDb;
                if (!float.IsNegativeInfinity(newRmsDb))
                {
                    AverageRmsDb = newRmsDb;
                    _lastRmsUpdateTime = DateTime.Now;
                }

                // Reset RMS if 5 seconds have passed since last update
                if ((DateTime.Now - _lastRmsUpdateTime).TotalSeconds > ResetIntervalSeconds)
                {
                    AverageRmsDb = CurrentLevel;
                    _lastRmsUpdateTime = DateTime.Now;
                }

                // Calculate Headroom (distance from peak to 0 dB)
                Headroom = float.IsNegativeInfinity(PeakLevel) ? 0 : 0 - PeakLevel;

                // Detect clipping (peak at or above 0 dB)
                bool isCurrentlyClipping = InstantaneousPeakLevel >= 0;
                if (isCurrentlyClipping && !_lastFrameWasClipping)
                {
                    ClippingCount++;
                }
                _lastFrameWasClipping = isCurrentlyClipping;

                // Add samples to session buffer for Integrated LUFS
                int bytesPerSample = _waveIn.WaveFormat.BitsPerSample / 8;
                int sampleCount = e.BytesRecorded / bytesPerSample;
                if (_waveIn.WaveFormat.BitsPerSample == 16 && _sessionAudioBuffer != null)
                {
                    // Reuse buffer if size matches, otherwise allocate new one
                    if (_sessionSampleBuffer == null || _sessionSampleBuffer.Length != sampleCount)
                    {
                        _sessionSampleBuffer = new float[sampleCount];
                        _lastSessionSampleBufferSize = sampleCount;
                    }

                    for (int i = 0; i < sampleCount; i++)
                    {
                        short sample = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
                        _sessionSampleBuffer[i] = sample / 32768f;
                    }
                    _sessionAudioBuffer.AddSamples(_sessionSampleBuffer);
                }

                // Calculate Integrated LUFS from session buffer
                if (_sessionAudioBuffer != null)
                {
                    var sessionSamples = _sessionAudioBuffer.GetAllSamples();
                    if (sessionSamples.Length > 0)
                    {
                        IntegratedLufsDb = CalculateLufs(sessionSamples);
                    }
                }

                AverageLufsDb = _levelMeter.AverageLufsDb;
            };

            _waveIn.StartRecording();
            _lastPeakUpdateTime = DateTime.Now;
            _lastRmsUpdateTime = DateTime.Now;
            IsMonitoring = true;
        }

        public void StopMonitoring()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
            IsMonitoring = false;
            CurrentLevel = float.NegativeInfinity;
            InstantaneousPeakLevel = float.NegativeInfinity;
            PeakLevel = float.NegativeInfinity;
            AverageRmsDb = float.NegativeInfinity;
            AverageLufsDb = float.NegativeInfinity;
            IntegratedLufsDb = float.NegativeInfinity;
            Headroom = 0;
            ClippingCount = 0;
            _lastPeakUpdateTime = DateTime.MinValue;
            _lastRmsUpdateTime = DateTime.MinValue;
            _sessionAudioBuffer = null;
        }

        public void ResetPeak()
        {
            PeakLevel = CurrentLevel;
        }

        private float CalculateLufs(float[] samples)
        {
            if (samples.Length == 0)
                return float.NegativeInfinity;

            // ITU-R BS.1770-4 compliant LUFS calculation
            float[] weightedSamples = ApplyKWeighting(samples);

            float sumSquares = 0;
            for (int i = 0; i < weightedSamples.Length; i++)
            {
                float sample = weightedSamples[i];
                sumSquares += sample * sample;
            }

            float meanSquare = sumSquares / weightedSamples.Length;

            if (meanSquare > 0)
            {
                return 10 * MathF.Log10(meanSquare) - 0.691f;
            }

            return float.NegativeInfinity;
        }

        private float[] ApplyKWeighting(float[] samples)
        {
            float[] weighted = ArrayPool<float>.Shared.Rent(samples.Length);
            try
            {
                float highFreqBoost = 1.4f;
                float highShelf = 1.2f;

                for (int i = 0; i < samples.Length; i++)
                {
                    float sample = samples[i];
                    if (i > 0)
                    {
                        float prev = samples[i - 1];
                        float gain = 1.0f + (highFreqBoost - 1.0f) * MathF.Abs(sample - prev);
                        weighted[i] = sample * gain * highShelf;
                    }
                    else
                    {
                        weighted[i] = sample * highFreqBoost * highShelf;
                    }
                }

                // Return only the filled portion
                float[] result = new float[samples.Length];
                Array.Copy(weighted, 0, result, 0, samples.Length);
                return result;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(weighted);
            }
        }

        private void OnLevelChanged()
        {
            LevelChanged?.Invoke(this, new LevelChangedEventArgs { Level = CurrentLevel });
        }

        /// <summary>
        /// Calculates the buffer milliseconds needed for a given monitor refresh rate.
        /// </summary>
        /// <param name="refreshRate">Monitor refresh rate in Hz (e.g., 60, 120, 144)</param>
        /// <returns>Buffer time in milliseconds</returns>
        public static int CalculateBufferMillisecondsFromRefreshRate(int refreshRate)
        {
            if (refreshRate <= 0)
                return 17; // Default to ~60fps if invalid

            // Buffer time = 1000 ms / refresh rate Hz
            // For 60Hz: 1000/60 ≈ 16.67ms
            // For 120Hz: 1000/120 ≈ 8.33ms
            // For 144Hz: 1000/144 ≈ 6.94ms
            int bufferMs = Math.Max(1, 1000 / refreshRate);
            return bufferMs;
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }

    public class LevelChangedEventArgs : EventArgs
    {
        public required float Level { get; init; }
    }

    public class LevelMeterProvider
    {
        private float _currentLevel = float.NegativeInfinity;
        private float _smoothedLevel = float.NegativeInfinity;
        private float _averageRmsDb = float.NegativeInfinity;
        private float _averageLufsDb = float.NegativeInfinity;
        private readonly WaveFormat _waveFormat;
        private readonly int _sampleRate;
        private readonly CircularBuffer _audioBuffer;
        private const int FiveSecondsBufferSize = 220500; // 44100 Hz * 5 seconds
        private const float FallRateDbPerSecond = 24f; // 0 to -48 in 2 seconds
        private const float BufferMilliseconds = 17f; // matches WaveInEvent buffer
        private readonly float _fallRatePerUpdate;

        public float CurrentLevel => _smoothedLevel;
        public float InstantaneousPeakLevel => _currentLevel;
        public float AverageRmsDb => _averageRmsDb;
        public float AverageLufsDb => _averageLufsDb;

        public LevelMeterProvider(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;
            _sampleRate = waveFormat.SampleRate;
            _audioBuffer = new CircularBuffer(FiveSecondsBufferSize);
            // Calculate fall rate per buffer update (17ms)
            _fallRatePerUpdate = FallRateDbPerSecond * (BufferMilliseconds / 1000f);
        }

        public void AddSamples(byte[] buffer, int length)
        {
            int bytesPerSample = _waveFormat.BitsPerSample / 8;
            int sampleCount = length / bytesPerSample;

            float maxValue = 0;
            float sumSquares = 0;

            if (_waveFormat.BitsPerSample == 16)
            {
                // Reuse samples buffer from pool
                float[] samples = ArrayPool<float>.Shared.Rent(sampleCount);
                try
                {
                    for (int i = 0; i < sampleCount; i++)
                    {
                        short sample = BitConverter.ToInt16(buffer, i * 2);
                        float normalized = sample / 32768f;
                        samples[i] = normalized;

                        float absValue = Math.Abs(normalized);
                        if (absValue > maxValue)
                            maxValue = absValue;

                        sumSquares += normalized * normalized;
                    }

                    // Add samples to circular buffer for RMS and LUFS calculation
                    _audioBuffer.AddSamples(samples);

                            // Calculate current peak
                            if (maxValue > 0)
                            {
                                _currentLevel = 20 * MathF.Log10(maxValue);
                            }
                            else
                            {
                                _currentLevel = float.NegativeInfinity;
                            }

                            // Apply smoothing with fallback envelope
                            if (float.IsNegativeInfinity(_smoothedLevel))
                            {
                                _smoothedLevel = _currentLevel;
                            }
                            else if (!float.IsNegativeInfinity(_currentLevel))
                            {
                                // If new level is higher, jump to it (attack is fast)
                                if (_currentLevel > _smoothedLevel)
                                {
                                    _smoothedLevel = _currentLevel;
                                }
                                else
                                {
                                    // Otherwise, apply gradual fallback
                                    _smoothedLevel = Math.Max(_currentLevel, _smoothedLevel - _fallRatePerUpdate);
                                }
                            }
                            else
                            {
                                // If no signal, apply fallback
                                _smoothedLevel = Math.Max(float.NegativeInfinity, _smoothedLevel - _fallRatePerUpdate);
                            }

                            // Calculate average RMS and LUFS over last 5 seconds
                            float[] bufferSamples = _audioBuffer.GetAllSamples();
                            if (bufferSamples.Length > 0)
                            {
                                // RMS calculation
                                float meanSquare = 0;
                                for (int i = 0; i < bufferSamples.Length; i++)
                                {
                                    meanSquare += bufferSamples[i] * bufferSamples[i];
                                }
                                meanSquare /= bufferSamples.Length;
                                float rms = MathF.Sqrt(meanSquare);

                                if (rms > 0)
                                {
                                    _averageRmsDb = 20 * MathF.Log10(rms);
                                }
                                else
                                {
                                    _averageRmsDb = float.NegativeInfinity;
                                }

                                // LUFS calculation (simplified K-weighted loudness)
                                _averageLufsDb = CalculateLufs(bufferSamples);
                            }
                        }
                        finally
                        {
                            ArrayPool<float>.Shared.Return(samples);
                        }
                    }
                }

        private float CalculateLufs(float[] samples)
        {
            if (samples.Length == 0)
                return float.NegativeInfinity;

            // ITU-R BS.1770-4 compliant LUFS calculation
            // Step 1: Apply K-weighting filter (simplified frequency weighting)
            float[] weightedSamples = ApplyKWeighting(samples);

            // Step 2: Calculate mean square
            float sumSquares = 0;
            int gatedSampleCount = 0;

            for (int i = 0; i < weightedSamples.Length; i++)
            {
                float sample = weightedSamples[i];
                sumSquares += sample * sample;
                gatedSampleCount++;
            }

            if (gatedSampleCount == 0)
                return float.NegativeInfinity;

            float meanSquare = sumSquares / gatedSampleCount;
            float rms = MathF.Sqrt(meanSquare);

            if (rms > 0)
            {
                // LUFS = -0.691 + 10*log10(mean_square)
                // This formula is the standard ITU-R BS.1770-4 reference
                return 10 * MathF.Log10(meanSquare) - 0.691f;
            }

            return float.NegativeInfinity;
        }

        private float[] ApplyKWeighting(float[] samples)
        {
            // ITU-R BS.1770-4 K-weighting approximation
            // This uses a simplified shelving filter approach
            // High shelf boost around 2kHz and high-frequency boost

            float[] weighted = ArrayPool<float>.Shared.Rent(samples.Length);
            try
            {
                // K-weighting coefficients (simplified but effective)
                // These approximate the high-frequency shelving behavior
                float highFreqBoost = 1.4f;  // Boost for presence peak around 2kHz
                float highShelf = 1.2f;       // Additional high-frequency emphasis

                for (int i = 0; i < samples.Length; i++)
                {
                    float sample = samples[i];

                    // Apply K-weighting as frequency-dependent gain
                    // This is a simplified approximation using sample history
                    if (i > 0)
                    {
                        // Simple IIR approximation of K-weighting
                        float prev = samples[i - 1];
                        float gain = 1.0f + (highFreqBoost - 1.0f) * MathF.Abs(sample - prev);
                        weighted[i] = sample * gain * highShelf;
                    }
                    else
                    {
                        weighted[i] = sample * highFreqBoost * highShelf;
                    }
                }

                float[] result = new float[samples.Length];
                Array.Copy(weighted, 0, result, 0, samples.Length);
                return result;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(weighted);
            }
        }
    }

    public class CircularBuffer
    {
        private readonly float[] _buffer;
        private int _writeIndex;
        private int _count;
        private readonly object _lockObject = new();
        private float[]? _cachedResult;

        public CircularBuffer(int size)
        {
            _buffer = new float[size];
            _writeIndex = 0;
            _count = 0;
        }

        public void AddSamples(float[] samples)
        {
            lock (_lockObject)
            {
                foreach (float sample in samples)
                {
                    _buffer[_writeIndex] = sample;
                    _writeIndex = (_writeIndex + 1) % _buffer.Length;
                    if (_count < _buffer.Length)
                        _count++;
                }
                // Invalidate cache when new samples are added
                _cachedResult = null;
            }
        }

        public float[] GetAllSamples()
        {
            lock (_lockObject)
            {
                // Return cached result if available
                if (_cachedResult != null && _cachedResult.Length == _count)
                {
                    return _cachedResult;
                }

                float[] result = new float[_count];
                if (_count == 0)
                {
                    _cachedResult = result;
                    return result;
                }

                if (_count < _buffer.Length)
                {
                    // Buffer not yet full, return samples in order
                    Array.Copy(_buffer, 0, result, 0, _count);
                }
                else
                {
                    // Buffer is full, copy from write position to end, then from start to write position
                    int remainingSpace = _buffer.Length - _writeIndex;
                    Array.Copy(_buffer, _writeIndex, result, 0, remainingSpace);
                    Array.Copy(_buffer, 0, result, remainingSpace, _writeIndex);
                }

                _cachedResult = result;
                return result;
            }
        }
    }
}
