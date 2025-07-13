﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TourPlanner.UI.Converters
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return false;

            bool isVisible = (bool)value;
            // Return 1* if visible, 0 if not.
            return isVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
