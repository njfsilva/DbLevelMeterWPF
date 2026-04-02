using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF
{
    public class RedLevelHeightConverter : IValueConverter
    {
        private const float MaxHeight = 240;
        private const float RedMin = -10;
        private const float RedMax = 0;
        private const float MinDbValue = -72;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is float dbValue)
            {
                if (float.IsNegativeInfinity(dbValue) || dbValue < MinDbValue)
                    return 0.0;

                // If level is below red range, show nothing
                if (dbValue < RedMin)
                    return 0.0;

                // If level is above red range, show full red bar
                if (dbValue >= RedMax)
                    return MaxHeight / 3;

                // Map dB range to height (partial fill)
                float normalizedValue = (dbValue - RedMin) / (RedMax - RedMin);
                return (normalizedValue * MaxHeight / 3);
            }

            return 0.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
