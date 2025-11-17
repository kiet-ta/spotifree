using System;
using System.Globalization;
using System.Windows.Data;

namespace Spotifree.Converters
{
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0:00";

            int totalSeconds;

            if (value is double d)
            {
                if (double.IsNaN(d) || double.IsInfinity(d)) return "0:00";
                totalSeconds = (int)Math.Round(d);
            }
            else if (value is float f)
            {
                totalSeconds = (int)Math.Round(f);
            }
            else if (value is int i)
            {
                totalSeconds = i;
            }
            else
            {
                return "0:00";
            }

            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
