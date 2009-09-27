using System;
using System.Windows.Data;

namespace TMSPS.TRC.BL.Wpf.Converters
{
    [ValueConversion(typeof(ushort), typeof(string))]
    public class RpcPortConverter : IValueConverter
    {
        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                return null;

            string stringValue = Convert.ToString(value);
            return stringValue == "0" ? null : stringValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(ushort))
                return null;

            ushort parsedValue;
            ushort.TryParse(Convert.ToString(value), out parsedValue);

            return parsedValue;
        }

        #endregion
    }
}