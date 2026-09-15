using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.ServiceExtensions;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Views.SistemDbYonetim
{
    public sealed partial class SistemDbYonetimView : Page
    {
        public SistemDbYonetimView()
        {
            ViewModel = ServiceLocator.Current.GetService<SistemDbYonetimViewModel>();
            InitializeContext();
            InitializeComponent();
            SurumFooter.Text = Helpers.AppSurumBilgisi.Metin;
        }

        public SistemDbYonetimViewModel ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Splash yönlendirmesi: karar izi (string) veya eski migration bayrağı (bool)
            if (e.Parameter is ShellArgs sa && sa.Parameter is string kararOzeti && !string.IsNullOrWhiteSpace(kararOzeti))
                ViewModel.SetMigrationMode(kararOzeti);
            else if (e.Parameter is ShellArgs sa2 && sa2.Parameter is bool migrationMode && migrationMode)
                ViewModel.SetMigrationMode();

            // Uygulama teması — pencere kontrolleri aktif temaya göre çizilir (ayarlanabilir tema).
            try { Helpers.TitleBarHelper.UpdateTitleBar(App.ThemeSelectorService.Theme); } catch { }

            var nav = ServiceLocator.Current.GetService<INavigationService>();
            if (nav != null && Frame != null)
                nav.Initialize(Frame);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try { Helpers.TitleBarHelper.UpdateTitleBar(App.ThemeSelectorService.Theme); } catch { }
        }

        public void InitializeContext()
        {
            var contextService = ServiceLocator.Current.GetService<IContextService>();
            contextService.InitializeWithContext(dispatcher: DispatcherQueue, viewElement: this);
        }

        /// <summary>Kural 17 Katman 2: ana border gölgesi — receiver Loaded'da (ctor'da değil; Splash emsali).</summary>
        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                AnaBorderShadow.Receivers.Add(RootGrid);
            }
            catch { }
        }

        private async void OnYardimClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new Views.Components.YardimDialog();
            dialog.IcerikAta("Sistem Veritabanı Yönetimi — Yardım", new List<Views.Components.YardimMaddesi>
            {
                new() { Baslik = "Bu sayfa ne işe yarar?", Aciklama = "Sistem veritabanının (Sistem.db) sağlık durumu burada izlenir ve yönetilir: durum, yedekleme, güncelleme, tanı ve işlem günlüğü." },
                new() { Baslik = "Durum ve işlemler", Aciklama = "Boyut, sürüm, tablo sayısı ve doğrulama sonucu. 'Sistemi Analiz Et' sağlığı yeniden ölçer; veritabanı hasarlıysa 'Onar' silip sıfırdan kurar (veriler kaybolur, onay istenir)." },
                new() { Baslik = "Yedekleme ve geri yükleme", Aciklama = "'Şimdi Yedekle' anlık yedek alır (en fazla N adet saklanır, sayısı Ayarlar'daki ayardan okunur; fazlası listeyi açınca otomatik temizlenir). Her satırda tarih-saat birincildir (dosya adı ipucunda). Satırdaki 'Geri Yükle' tek kapıdan geçer: yedek önce analiz edilir (bozuk dosya / yeni sürüm / kayıp kayıt) ve hüküm ile geri yüklemede kaybolacak kayıtlar gösterilir; kayıp varsa 6 haneli onay kodu istenir, bozuk veya daha yeni sürümlü yedekte buton kapalıdır. Onaydan sonra geri yükleme tek seferde yapılır — işlem geri alınamaz." },
                new() { Baslik = "Sistem güncellemesi", Aciklama = "Bekleyen göç listesi. 'Güncellemeyi Uygula' önce yedek alır, göçü uygular, sonucu doğrular (1/3→3/3). Liste boşsa sistem günceldir." },
                new() { Baslik = "Tanı ve kurulum kaydı", Aciklama = "Bileşen testleri ve bu kurulumun kimliği (yedeklere damgalanır). Sorun çıktığında destek ile paylaşılır." },
                new() { Baslik = "Giriş Ekranına Devam Et", Aciklama = "Yalnızca veritabanı hazır olduğunda görünür. Hazır değilse önce kurulum/onarım yapılmalıdır." },
            });
            await Helpers.DialogHelper.ShowCenteredAsync(dialog);
        }

    }
}
