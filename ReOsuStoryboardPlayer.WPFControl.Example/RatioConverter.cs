using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReOsuStoryboardPlayer.WPFControl.Example
{
    public class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is double d)
            {
                return System.Convert.ToDouble(value) * d;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}