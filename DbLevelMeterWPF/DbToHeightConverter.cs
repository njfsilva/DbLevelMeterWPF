using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF
{
    public class DbToHeightConverter : IValueConverter
    {
        private const float MaxHeight = 240;
        private const float MinDb = -72;
        private const float MaxDb = 0;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is float dbValue)
            {
                if (float.IsNegativeInfinity(dbValue))
                    return 0.0;

                // Clamp the value between MinDb and MaxDb
                dbValue = Math.Clamp(dbValue, MinDb, MaxDb);

                // Map dB range (-72 to 0) to height (0 to MaxHeight)
                float normalizedValue = (dbValue - MinDb) / (MaxDb - MinDb);
                return normalizedValue * MaxHeight;
            }

            return 0.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
