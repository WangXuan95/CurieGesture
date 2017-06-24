using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BLEServiceBrowser.Converters
{
    /*
     * Converter to change a True/False value to a Visible/Collapsed value.
     * This allows the visibility of controls to be bound to viewmodels.
     */
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool visibility = (bool)value;

            if (parameter != null)
            {
                if (parameter.ToString() == StringResources.Invert.ToLower())
                {
                    visibility = !visibility;
                }
            }
            return (visibility) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
