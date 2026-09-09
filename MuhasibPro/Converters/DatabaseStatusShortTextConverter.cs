using Microsoft.UI.Xaml.Data;
using MuhasibPro.Domain.Enum.DatabaseEnum;

namespace MuhasibPro.Converters
{
    /// <summary>
    /// DatabaseStatusResult enum -> kısa net rozet metni (Güncel / Güncelleme Gerekli / Dosya Yok / Kontrol Gerekli)
    /// Uzun GetStatusMessage() yerine kart üzerinde kompakt gösterim için.
    /// </summary>
    public class DatabaseStatusShortTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DatabaseStatusResult status)
            {
                return status switch
                {
                    DatabaseStatusResult.Healty => "Güncel",
                    DatabaseStatusResult.RequiredUpdating => "Güncelleme Gerekli",
                    DatabaseStatusResult.DatabaseNotFound => "Dosya Yok",
                    DatabaseStatusResult.ConnectionFailed => "Bağlantı Yok",
                    DatabaseStatusResult.InvalidSchema => "Onarım Gerekli",
                    DatabaseStatusResult.UnknownError => "Kontrol Gerekli",
                    DatabaseStatusResult.RestoreCompleted => "Geri Yüklendi",
                    _ => "Durum yok"
                };
            }
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                // Fallback: uzun mesaj geldiyse kısalt (ilk 32 karakter + ...)
                if (s.Length > 36) return s.Substring(0, 36) + "…";
                return s;
            }
            return "Durum yok";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
