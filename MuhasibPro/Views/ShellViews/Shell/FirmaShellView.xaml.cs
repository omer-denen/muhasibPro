using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Contracts.UIService;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.ShellViews.Shell
{
    /// <summary>Firma ve mali dönem seçim ekranı.</summary>
    public sealed partial class FirmaShellView : Page
    {
        private IThemeSelectorService? _temaServisi;
        private bool _temaSenkronize;
        private bool _temaKaydediliyor;

        public FirmaShellView()
        {
            ViewModel = ServiceLocator.Current.GetService<FirmaShellViewModel>();
            InitializeComponent();
            SurumFooter.Text = Helpers.AppSurumBilgisi.Metin;
            Unloaded += OnPageUnloaded;
        }

        public FirmaShellViewModel ViewModel { get;}

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
            TemaKisayolBaslat();
        }

        private void OnPageUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_temaServisi != null)
                _temaServisi.ThemeChanged -= OnTemaDegisti;
        }

        /// <summary>Tema kısayolu: canlı temayla senkron başlar, dışarıdan değişimi dinler.</summary>
        private void TemaKisayolBaslat()
        {
            _temaServisi = ServiceLocator.Current.GetService<IThemeSelectorService>();
            if (_temaServisi == null)
            {
                TemaPill.Visibility = Visibility.Collapsed;
                return;
            }
            TemaToggleSenkronize(_temaServisi.Theme);
            _temaServisi.ThemeChanged += OnTemaDegisti;
        }

        private void OnTemaDegisti(object sender, ElementTheme theme)
        {
            // Olay arka plandan gelebilir (model-kayıt yolu) → UI kuyruğunda senkronize et.
            DispatcherQueue.TryEnqueue(() => TemaToggleSenkronize(theme));
        }

        /// <summary>Kapalı=sol=Açık, Açık=sağ=Koyu. Sistem (Default) ikiliye sığmaz → efektif tema gösterilir.</summary>
        private void TemaToggleSenkronize(ElementTheme theme)
        {
            _temaSenkronize = true;
            try
            {
                TemaToggle.IsOn = theme switch
                {
                    ElementTheme.Light => false,
                    ElementTheme.Dark => true,
                    _ => ActualTheme == ElementTheme.Dark,
                };
            }
            finally
            {
                _temaSenkronize = false;
            }
        }

        /// <summary>Anahtar konumu → model kaydeder (Görünüm ile aynı kaynak); canlı uygulama olayla gelir.</summary>
        private async void OnTemaToggleToggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_temaSenkronize || sender is not ToggleSwitch sw)
                return;
            if (_temaKaydediliyor || _temaServisi == null)
            {
                TemaToggleSenkronize(_temaServisi?.Theme ?? ElementTheme.Default);
                return;
            }
            await TemaModelKaydetAsync(sw.IsOn ? "Dark" : "Light");
        }

        private async System.Threading.Tasks.Task TemaModelKaydetAsync(string tag)
        {
            _temaKaydediliyor = true;
            try
            {
                var saglayici = ServiceLocator.Current.GetService<IAppPlatformSettingsProvider>();
                var ayarlar = await saglayici.GetAsync();
                if (ayarlar.ThemeDefault == tag)
                    return;
                ayarlar.ThemeDefault = tag;
                await saglayici.SaveAsync(ayarlar);
            }
            catch (Exception ex)
            {
                TemaToggleSenkronize(_temaServisi?.Theme ?? ElementTheme.Default);
                ServiceLocator.Current.GetService<INotificationService>()
                    ?.Show("Tema değiştirilemedi", ex.Message, NotificationType.Warning);
            }
            finally
            {
                _temaKaydediliyor = false;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as ShellArgs ?? new ShellArgs();
            ViewModel.BaseSubscribe();
            await ViewModel.LoadAsync(args);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.BaseUnsubscribe();
            ViewModel.FirmalarVM.Unload();
        }

        /// <summary>Ayarlar penceresini ayrı modal pencerede açar.</summary>
        private async void OnDenetimMasasiClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.NavigationService.CreateNewViewAsync<DenetimMasasiViewModel>(null, "Ayarlar");
        }
    }
}
