using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdminDesktop.Converters;

public class BoolToVisibility : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => value is Visibility.Visible;
}

public class InverseBoolToVisibility : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => value is Visibility.Collapsed;
}

public class NullToVisibility : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is string s ? (string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible)
           : (value != null ? Visibility.Visible : Visibility.Collapsed);
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotSupportedException();
}

public class EqualConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value?.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotSupportedException();
}
