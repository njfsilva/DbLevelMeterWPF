using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace DbLevelMeterWPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly AudioLevelMonitor _levelMonitor;
        private int _selectedDeviceIndex;
        private bool _isMonitoring;
        private float _currentLevelDb;
        private float _peakLevelDb;
        private float _averageRmsDb;
        private float _averageLufsDb;
        private float _integratedLufsDb;
        private float _headroom;
        private int _clippingCount;
        private RelayCommand? _resetPeakCommand;

        public ObservableCollection<string> AvailableDevices => _levelMonitor.AvailableDevices;

        public int SelectedDeviceIndex
        {
            get => _selectedDeviceIndex;
            set
            {
                if (SetProperty(ref _selectedDeviceIndex, value))
                {
                    if (value >= 0 && value < _levelMonitor.AvailableDevices.Count)
                    {
                        // Save the selected device for next startup
                        AppSettings.LastDeviceIndex = value;

                        // Stop current monitoring and start with new device
                        if (IsMonitoring)
                        {
                            _levelMonitor.StopMonitoring();
                        }
                        int bufferMs = GetAdaptiveBufferMilliseconds();
                        _levelMonitor.StartMonitoring(value, bufferMs);
                        IsMonitoring = true;
                    }
                }
            }
        }

        public bool IsMonitoring
        {
            get => _isMonitoring;
            private set => SetProperty(ref _isMonitoring, value);
        }

        public float CurrentLevelDb
        {
            get => _currentLevelDb;
            private set => SetProperty(ref _currentLevelDb, value);
        }

        public float PeakLevelDb
        {
            get => _peakLevelDb;
            private set => SetProperty(ref _peakLevelDb, value);
        }

        public float AverageRmsDb
        {
            get => _averageRmsDb;
            private set => SetProperty(ref _averageRmsDb, value);
        }

        public float AverageLufsDb
        {
            get => _averageLufsDb;
            private set => SetProperty(ref _averageLufsDb, value);
        }

        public float IntegratedLufsDb
        {
            get => _integratedLufsDb;
            private set => SetProperty(ref _integratedLufsDb, value);
        }

        public float Headroom
        {
            get => _headroom;
            private set => SetProperty(ref _headroom, value);
        }

        public int ClippingCount
        {
            get => _clippingCount;
            private set => SetProperty(ref _clippingCount, value);
        }

        public RelayCommand ResetPeakCommand => _resetPeakCommand ??= new RelayCommand(
            _ => ResetPeak(),
            _ => IsMonitoring
        );

        public MainWindowViewModel()
        {
            _levelMonitor = new AudioLevelMonitor();
            _levelMonitor.LevelChanged += (s, e) =>
            {
                CurrentLevelDb = float.IsNegativeInfinity(e.Level) ? -72 : e.Level;
                PeakLevelDb = float.IsNegativeInfinity(_levelMonitor.PeakLevel) ? -72 : _levelMonitor.PeakLevel;
                AverageRmsDb = float.IsNegativeInfinity(_levelMonitor.AverageRmsDb) ? -72 : _levelMonitor.AverageRmsDb;
                AverageLufsDb = float.IsNegativeInfinity(_levelMonitor.AverageLufsDb) ? -72 : _levelMonitor.AverageLufsDb;
                IntegratedLufsDb = float.IsNegativeInfinity(_levelMonitor.IntegratedLufsDb) ? -72 : _levelMonitor.IntegratedLufsDb;
                Headroom = _levelMonitor.Headroom;
                ClippingCount = _levelMonitor.ClippingCount;
            };

            // Load the last selected device index from settings
            _selectedDeviceIndex = AppSettings.LastDeviceIndex;

            // Start monitoring with the last device if available
            int bufferMs = GetAdaptiveBufferMilliseconds();
            if (_levelMonitor.AvailableDevices.Count > 0 && _selectedDeviceIndex >= 0 && _selectedDeviceIndex < _levelMonitor.AvailableDevices.Count)
            {
                _levelMonitor.StartMonitoring(_selectedDeviceIndex, bufferMs);
                IsMonitoring = true;
            }
            else if (_levelMonitor.AvailableDevices.Count > 0)
            {
                // If saved index is invalid, use the first device
                _selectedDeviceIndex = 0;
                _levelMonitor.StartMonitoring(0, bufferMs);
                IsMonitoring = true;
            }
        }

        private void ResetPeak()
        {
            _levelMonitor.ResetPeak();
            PeakLevelDb = CurrentLevelDb;
        }

        /// <summary>
        /// Gets the adaptive buffer milliseconds based on the monitor's refresh rate.
        /// </summary>
        private int GetAdaptiveBufferMilliseconds()
        {
            try
            {
                // Try to get refresh rate from the primary screen
                var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
                if (primaryScreen?.DeviceName != null)
                {
                    // Get the display device info - this is a Windows API approach
                    // Using a simpler fallback method if the API doesn't provide refresh rate directly
                    int refreshRate = GetMonitorRefreshRate(primaryScreen.DeviceName);
                    if (refreshRate > 0)
                    {
                        return AudioLevelMonitor.CalculateBufferMillisecondsFromRefreshRate(refreshRate);
                    }
                }
            }
            catch
            {
                // If detection fails, fall back to default
            }

            // Default to 60Hz (17ms) if detection fails
            return 17;
        }

        /// <summary>
        /// Gets the refresh rate of a monitor device.
        /// </summary>
        private int GetMonitorRefreshRate(string deviceName)
        {
            try
            {
                // Using Win32 API to get display device mode
                var dm = new DEVMODE();
                dm.dmSize = (ushort)Marshal.SizeOf(dm);

                if (EnumDisplaySettings(deviceName, -1, ref dm) != 0)
                {
                    return (int)dm.dmDisplayFrequency;
                }
            }
            catch
            {
                // If Windows API call fails, fall back to default
            }

            return 60; // Default to 60Hz if detection fails
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string lpszDeviceName,
            int iModeNum,
            ref DEVMODE lpDevMode);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public ushort dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public void Dispose()
        {
            _levelMonitor.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}
