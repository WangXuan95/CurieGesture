using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BLEServiceBrowser.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility visibility = Visibility.Collapsed;
            bool toShow = (bool)value;

            if (parameter != null)
            {
                if (parameter.ToString() == "true")
                {
                    toShow = !toShow;
                }
            }

            if (toShow)
            {
                visibility = Visibility.Visible;
            }
            return visibility;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
