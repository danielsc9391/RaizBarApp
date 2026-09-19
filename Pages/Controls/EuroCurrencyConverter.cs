using System.Globalization;

namespace RaizBarApp.Pages.Controls;

public sealed class EuroCurrencyConverter : IValueConverter
{
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-PT");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is decimal total
            ? $"Total: {total.ToString("C", PortugueseCulture)}"
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}