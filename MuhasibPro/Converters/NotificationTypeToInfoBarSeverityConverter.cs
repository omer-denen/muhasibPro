using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;

namespace MuhasibPro.Converters
{
    /// <summary>Bildirim türünü InfoBar severity'sine çevirir (renk/ikon sistem kaynağından).</summary>
    public class NotificationTypeToInfoBarSeverityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is NotificationType tur
                ? tur switch
                {
                    NotificationType.Success => InfoBarSeverity.Success,
                    NotificationType.Warning => InfoBarSeverity.Warning,
                    NotificationType.Danger => InfoBarSeverity.Error,
                    _ => InfoBarSeverity.Informational
                }
                : InfoBarSeverity.Informational;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
