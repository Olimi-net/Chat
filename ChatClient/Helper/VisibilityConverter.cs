using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.15
/// </created>

namespace ChatClient.Helper
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, 
            object parameter, CultureInfo culture)
        {
            bool arg = (bool) value;
            return arg ? Visibility.Visible :  Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, 
            object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
