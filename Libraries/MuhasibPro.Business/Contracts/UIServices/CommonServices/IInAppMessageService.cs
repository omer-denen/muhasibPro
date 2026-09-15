using System.Collections.ObjectModel;

namespace MuhasibPro.Business.Contracts.UIServices.CommonServices;

/// <summary>
/// Uygulama içi bildirim merkezi (SCOPED — pencere başına ayrı). UI host'ları <see cref="Mesajlar"/>
/// koleksiyonunu bağlar; aynı etiket+grup yenisini eskisinin yerine koyar. Bildirim yalnız
/// yayınlandığı pencerede görünür (diğer pencerelere yansımaz).
/// </summary>
public interface IInAppMessageService
{
    /// <summary>Aktif bildirimler (en yeni sonda; UI thread'de güncellenir).</summary>
    ObservableCollection<InAppBildirim> Mesajlar { get; }

    /// <summary>Bildirim yayınla (aynı etiket+grup varsa değiştirir).</summary>
    void Yayinla(InAppBildirim bildirim);

    /// <summary>Bildirimi Id ile kapat.</summary>
    void Kapat(string id);

    /// <summary>Tüm bildirimleri kapat.</summary>
    void Temizle();
}
