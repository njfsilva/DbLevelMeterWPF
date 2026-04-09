using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF
{
    public class YellowLevelHeightConverter : IValueConverter
    {
        private const float MaxHeight = 240;
        private const float YellowMin = -18;
        private const float YellowMax = -10;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is float dbValue)
            {
                if (float.IsNegativeInfinity(dbValue) || dbValue < YellowMin)
                    return 0.0;

                // If level is at or above yellow range max, show full yellow bar
                if (dbValue >= YellowMax)
                    return MaxHeight / 3;

                // Map dB range to height (partial fill)
                float normalizedValue = (dbValue - YellowMin) / (YellowMax - YellowMin);
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
