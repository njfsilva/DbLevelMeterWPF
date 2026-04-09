using System.Globalization;
using System.Windows.Data;

namespace DbLevelMeterWPF
{
    public class GreenLevelHeightConverter : IValueConverter
    {
        private const float MaxHeight = 240;
        private const float GreenMin = -72;
        private const float GreenMax = -18;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is float dbValue)
            {
                if (float.IsNegativeInfinity(dbValue) || dbValue < GreenMin)
                    return 0.0;

                // If level is at or above green range max, show full green bar
                if (dbValue >= GreenMax)
                    return MaxHeight / 3;

                // Map dB range to height (partial fill)
                float normalizedValue = (dbValue - GreenMin) / (GreenMax - GreenMin);
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
