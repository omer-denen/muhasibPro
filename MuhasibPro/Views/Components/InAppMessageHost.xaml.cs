using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Views.Components;

/// <summary>
/// OS toast yerine uygulama içi bildirimleri gösteren host (InfoBar).
/// Bilgi/Başarılı/Uyarı bildirimleri ~6 sn sonra oto-kapanır; Hata tipi kullanıcı kapatana kadar kalır.
/// </summary>
public sealed partial class InAppMessageHost : UserControl
{
    private static readonly TimeSpan OtoKapanma = TimeSpan.FromSeconds(6);
    private readonly IInAppMessageService _service;
    private readonly DispatcherTimer _timer;

    public InAppMessageHost()
    {
        _service = ServiceLocator.Current.GetService<IInAppMessageService>();
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => OtoKapananlariKapat();
        _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
    }

    public ObservableCollection<InAppBildirim> Mesajlar => _service.Mesajlar;

    private void OtoKapananlariKapat()
    {
        var simdi = DateTime.Now;
        var kapatilacak = Mesajlar
            .Where(m => m.Tur != NotificationType.Danger && simdi - m.Olusturma >= OtoKapanma)
            .Select(m => m.Id)
            .ToList();

        foreach (var id in kapatilacak)
            _service.Kapat(id);
    }

    private void OnCloseButtonClick(InfoBar sender, object args)
    {
        if (sender?.DataContext is InAppBildirim bildirim)
            _service.Kapat(bildirim.Id);
    }
}
