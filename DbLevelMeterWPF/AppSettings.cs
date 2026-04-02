using System.IO;
using System.Text.Json;

namespace DbLevelMeterWPF
{
    public static class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DbLevelMeterWPF",
            "settings.json"
        );

        private static SettingsData? _settings;

        public static int LastDeviceIndex
        {
            get => LoadSettings().LastDeviceIndex;
            set
            {
                var settings = LoadSettings();
                settings.LastDeviceIndex = value;
                SaveSettings(settings);
            }
        }

        private static SettingsData LoadSettings()
        {
            if (_settings != null)
                return _settings;

            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    _settings = new SettingsData();
                }
            }
            catch
            {
                _settings = new SettingsData();
            }

            return _settings;
        }

        private static void SaveSettings(SettingsData settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
                _settings = settings;
            }
            catch
            {
                // Silently fail if settings can't be saved
            }
        }

        private class SettingsData
        {
            public int LastDeviceIndex { get; set; } = 0;
        }
    }
}
