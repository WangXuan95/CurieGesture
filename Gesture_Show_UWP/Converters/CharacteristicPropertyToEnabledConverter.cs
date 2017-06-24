using System;
using Windows.UI.Xaml.Data;

namespace BLEServiceBrowser.Converters
{
    /*
     * Converter to take a comma seperated list of descriptors, cut them up and create an array from them.
     * The parameter value contains a string value, which should corrispond to the relevent descriptor.
     * For example if you have a "Read" button which you only want enabled if there is a read descriptor, the parameter 
     * should be set to "read" in the xaml.
     */ 
    public class CharacteristicPropertyToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var property = (string)value.ToString();

            string[] decriptors = property.Split(',');
            foreach (string item in decriptors)
            {
                if (item.ToLower().Trim() == parameter.ToString().ToLower().Trim())
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
