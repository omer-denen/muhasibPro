using MuhasibPro.HostBuilders;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Views.DenetimMasasi
{
    /// <summary>
    /// Denetim Masası — bağımsız pencere page'i (mini-MainShell: sol NavigationView + sağ Frame).
    /// FirmaShell'den ayrıldı (Oturum 244): modal DetailsWindow'da açılır, Kural 17 Katman 1 zemini taşır.
    /// ViewModel DI'dan çözülür; iç çerçeve pencerenin paylaşılan servisini ezmemesi için
    /// kendine özel NavigationService örneğiyle yönetilir (ShellView deseni).
    /// </summary>
    public sealed partial class DenetimMasasiView : Page
    {
        public DenetimMasasiViewModel ViewModel { get; }

        private readonly NavigationService _icNav = new();

        public DenetimMasasiView()
        {
            ViewModel = ServiceLocator.Current.GetService<DenetimMasasiViewModel>();
            InitializeComponent();
            _icNav.Initialize(IcerikFrame);
            ViewModel.PropertyChanged += OnViewModelChanged;
            Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            PencereBoyutunuAyarla();
            await ViewModel.LoadAsync();
            SeciliBolumeGit();
            NavSeciminiAynala();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= OnViewModelChanged;
            Unloaded -= OnUnloaded;
        }

        /// <summary>Ayarlar penceresi için makul boyut (ekran çalışma alanına sığdırılır).</summary>
        private void PencereBoyutunuAyarla()
        {
            try
            {
                var pencere = MuhasibPro.Helpers.WindowHelpers.WindowHelper.GetWindowForElement(this);
                if (pencere == null) return;
                var alan = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(pencere.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest)?.WorkArea;
                int genislik = Math.Min(1380, (alan?.Width ?? 1380) - 80);
                int yukseklik = Math.Min(880, (alan?.Height ?? 880) - 80);
                pencere.AppWindow.Resize(new Windows.Graphics.SizeInt32(Math.Max(genislik, 640), Math.Max(yukseklik, 480)));
            }
            catch { }
        }

        private void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DenetimMasasiViewModel.SeciliBolum))
            {
                SeciliBolumeGit();
                NavSeciminiAynala();
            }
            else if (e.PropertyName == nameof(DenetimMasasiViewModel.SeciliMenu))
            {
                NavSeciminiAynala();
            }
        }

        /// <summary>Nav seçim vurgusunu VM durumuyla eşitler. Menü kaynağı yenilendiğinde
        /// (arama/yükleme) seçili örnek aynı kalsa da görsel seçim düşer; bu yüzden aynı
        /// örnekte dahi null→hedef atamasıyla tazelenir (iki yön idempotent — döngü üretmez).</summary>
        private void NavSeciminiAynala()
        {
            var hedef = ViewModel.SeciliMenu;
            if (hedef == null)
                return;
            if (ReferenceEquals(NavKok.SelectedItem, hedef))
                NavKok.SelectedItem = null;
            NavKok.SelectedItem = hedef;
        }

        /// <summary>Kullanıcı nav'dan seçince VM bölümünü günceller (SelectedItem iki-yönlü aynasına ek güvence).</summary>
        private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is AyarlarNavigationMenu menu && menu.Bolum != ViewModel.SeciliBolum)
                ViewModel.SeciliBolum = menu.Bolum;
        }

        /// <summary>Seçili bölümü iç çerçevede açar (çocuk VM örneği parametreyle — tek örnek korunur).</summary>
        private void SeciliBolumeGit()
        {
            switch (ViewModel.SeciliBolum)
            {
                case AyarBolumu.GirisPaneli:
                    _icNav.Navigate<GirisDashboardViewModel>(ViewModel.GirisPaneli);
                    break;
                case AyarBolumu.Giris:
                    _icNav.Navigate<IdentityAyarlarViewModel>(ViewModel.Giris);
                    break;
                case AyarBolumu.Firma:
                    _icNav.Navigate<FirmaKayitAyarlarViewModel>(ViewModel.FirmaKayit);
                    break;
                case AyarBolumu.Donem:
                    _icNav.Navigate<DonemAyarlarViewModel>(ViewModel.Donem);
                    break;
                case AyarBolumu.Guncelleme:
                    // UpdateViewModel→UpdateView kaydı ana menüye ait; iç çerçeve doğrudan
                    // sayfaya gider (UpdateView VM'ini kendisi çözer, yeni VM/kayıt yok).
                    IcerikFrame.Navigate(typeof(Sayfalar.GuncellemeAyarSayfasi));
                    break;
                case AyarBolumu.Veritabani:
                    // Tek view: Sistem Veritabanı + Mali Dönem Veritabanları grupları.
                    // Sayfa, çocuk VM'leri DenetimMasasiViewModel üzerinden alır.
                    IcerikFrame.Navigate(typeof(Sayfalar.VeritabaniAyarSayfasi), ViewModel);
                    break;
                case AyarBolumu.GelistiriciAraclari:
                    // Yalnız DEBUG: çocuk VM örneği parametreyle (tek örnek korunur).
                    _icNav.Navigate<GelistiriciAraclariViewModel>(ViewModel.GelistiriciAraclari);
                    break;
                default:
                    _icNav.Navigate<AppPlatformAyarlarViewModel>(ViewModel.Gorunum);
                    break;
            }
            IcerikFrame.BackStack.Clear();
            IcerikFrame.ForwardStack.Clear();
        }

        private async void OnYardimClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Views.Components.YardimDialog();
            dialog.IcerikAta("Ayarlar — Yardım", new List<Views.Components.YardimMaddesi>
            {
                new() { Baslik = "Bu pencere ne işe yarar?", Aciklama = "Firma ve dönem seçiminden bağımsız, tüm uygulama ayarlarının tek yönetim penceresi. Firma seçiminden ayrı açılır; arkadaki seçim ekranı bu pencere açıkken kilitlidir." },
                new() { Baslik = "Giriş sayfası", Aciklama = "Üstte üç durum düğmesi: MuhasibPro (sürüm ve dağıtım bilgisi), Sistem Veritabanı (bağlantı durumu + dosya boyutu; tıklayınca Veritabanı bölümü açılır) ve Uygulama Güncelleme (son kontrol zamanı; tıklayınca Güncelleme bölümü açılır). Altında 'Kullanıcıya Ait Firmalar' bölümü yer alır." },
                new() { Baslik = "Firmalar ve mali dönemler", Aciklama = "Sağdaki 'Kullanıcıya Ait Firmalar' kartında her firmayı genişlettiğinizde o firmanın açık mali dönemleri (yıl + veritabanı adı) görünür. 'Gelişmiş Yönetim' ilgili firmanın Mali Dönem Yönetimi penceresini açar; 'Yeni Firma' yeni firma tanımlama formunu başlatır." },
                new() { Baslik = "Bölümler", Aciklama = "Sol menüde: Giriş (hesap, firma ve dönem özeti), Görünüm, Güvenlik, Firma, Veritabanı (Sistem Veritabanı + Mali Dönem Veritabanları grupları), Mali Dönem ve Güncelleme. Her bölüm kendi sayfasında açılır." },
                new() { Baslik = "Kapsam: kullanıcı ve firma", Aciklama = "Görünüm, Yedekleme ve Saklama ayarları kullanıcı bazlıdır (her kullanıcı kendi ayarını görür). Firma ve Mali Dönem ayarları global şablondur. Güvenlik ayarları yöneticiye özeldir." },
                new() { Baslik = "Kaydetme", Aciklama = "Ayar değişiklikleri seçildiği anda ilgili sağlayıcı üzerinden otomatik kaydedilir. Kritik ayarları yalnızca yönetici değiştirebilir; yetki yoksa değişiklik reddedilir ve uyarı gösterilir." },
                new() { Baslik = "Arama", Aciklama = "'Bir ayar bulun' kutusu bölüm menüsünü başlığa göre süzer; aramanın temizlenmesiyle tüm bölümler geri gelir." },
                new() { Baslik = "Geliştirici Araçları (yalnız geliştirme)", Aciklama = "Yalnız DEBUG derlemesinde görünen bölüm: kurulum kimliği onarım/sıfırlama, transfer taramasını elle tetikleme, dönem şema damgalarını görüntüleme, ayrıntılı log seviyesi ve log/veri klasörlerini açma. Üretim sürümünde bu bölüm yer almaz." },
            });
            await Helpers.DialogHelper.ShowCenteredAsync(dialog);
        }
    }
}
