using System;
using System.Globalization;
using System.Windows.Data;

namespace MyWorkTracker.Code
{
    class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return "Visible";
                else
                    return "Hidden";
            }
            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value.ToString().ToLower())
            {
                case "Hidden":
                    return false;
                case "Visible":
                    return true;
            }
            return false;
        }
    }
}
