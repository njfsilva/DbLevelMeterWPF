using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF
{
    public class DbToYPositionConverter : IValueConverter
    {
        private const float MaxHeight = 250;
        private const float MinDb = -72;
        private const float MaxDb = 0;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is float dbValue)
            {
                if (float.IsNegativeInfinity(dbValue) || dbValue < MinDb)
                    return MaxHeight;  // -∞ at the bottom

                // Clamp the value between MinDb and MaxDb
                dbValue = Math.Clamp(dbValue, MinDb, MaxDb);

                // Map dB range (-72 to 0) to Y position
                // The lower the dB (quieter), the higher the Y value (toward bottom)
                // Y = 250 at bottom (-72 dB), Y = 0 at top (0 dB)
                float normalizedValue = (dbValue + 72) / 72;
                return (1 - normalizedValue) * MaxHeight;
            }

            return MaxHeight;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
