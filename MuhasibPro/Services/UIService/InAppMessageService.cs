using System.Collections.ObjectModel;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;

namespace MuhasibPro.Services.UIService;

/// <summary>
/// Uygulama içi bildirim merkezi (Singleton). Koleksiyon UI thread'de tutulur;
/// aynı etiket+grup gelen bildirim eskisinin yerine geçer, en fazla <see cref="MaxMesaj"/> tutulur.
/// </summary>
public class InAppMessageService : IInAppMessageService
{
    private const int MaxMesaj = 3;
    private readonly ObservableCollection<InAppBildirim> _mesajlar = new();

    public ObservableCollection<InAppBildirim> Mesajlar => _mesajlar;

    public void Yayinla(InAppBildirim bildirim)
    {
        if (bildirim == null) return;
        Dispatch(() =>
        {
            var ayni = _mesajlar.FirstOrDefault(m =>
                m.Etiket == bildirim.Etiket && m.Grup == bildirim.Grup);
            if (ayni != null)
                _mesajlar.Remove(ayni);

            _mesajlar.Add(bildirim);
            while (_mesajlar.Count > MaxMesaj)
                _mesajlar.RemoveAt(0);
        });
    }

    public void Kapat(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        Dispatch(() =>
        {
            var mevcut = _mesajlar.FirstOrDefault(m => m.Id == id);
            if (mevcut != null)
                _mesajlar.Remove(mevcut);
        });
    }

    public void Temizle() => Dispatch(() => _mesajlar.Clear());

    private static void Dispatch(Action action)
    {
        var queue = App._dispatcherQueue;
        if (queue == null || queue.HasThreadAccess)
            action();
        else
            queue.TryEnqueue(() => action());
    }
}
