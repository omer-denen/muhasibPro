using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MuhasibPro.Converters;

public sealed class BoolToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; }
    public Brush FalseBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool && (bool)value;
        return boolValue ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
