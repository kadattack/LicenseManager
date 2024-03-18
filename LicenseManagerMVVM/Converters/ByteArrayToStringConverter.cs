using System.Security.Cryptography;

namespace LicenseManagerMVVM.Converters;



using System;
using System.Globalization;
using Avalonia.Data.Converters;

public class ByteArrayToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] byteArray)
        {
            return System.Convert.ToBase64String(byteArray);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
