using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TourPlanner.UI.Converters
{
    public class MinutesToHourMinuteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double minutes)
            {
                var time = TimeSpan.FromMinutes(minutes);
                return $"{(int)time.TotalHours}h {time.Minutes:D2} min";
            }

            return "0h 00min";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
