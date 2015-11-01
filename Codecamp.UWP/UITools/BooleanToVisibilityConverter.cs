using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Codecamp.UWP.UITools
{
    public class BooleanToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isTrue = (bool) value;
            return isTrue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var visibility = (Visibility)value;
            return visibility == Visibility.Visible;
        }
    }
}
