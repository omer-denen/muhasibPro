using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.ViewModels.Loggings.SistemLogs;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class MainMenuViewModel : ShellViewModel
    {
        private readonly IPermissionService _yetki;

        public MainMenuViewModel(
            IAuthenticationService authenticationService,
            ISistemDatabaseService sistemDatabaseService,
            ICommonServices commonServices,
            IPermissionService permissionService = null) : base(
            authenticationService,
            sistemDatabaseService,
            commonServices)
        {
            _yetki = permissionService;
        }


        private ObservableCollection<NavigationItem> _navigationItems;

        public ObservableCollection<NavigationItem> NavigationItems
        {
            get => _navigationItems;
            set => Set(ref _navigationItems, value);
        }

        /// <summary>K4: kullanıcının erişebildiği menü öğeleri (navbar bunu gösterir).
        /// Standart (Kural 14/19 araştırma): yetkisiz menü öğesi **gizlenir** (tease-and-block yok).
        /// Yükleme öncesi boştur; `YetkileriUygulaAsync` doldurur.</summary>
        public ObservableCollection<NavigationItem> GorunurNavigationItems { get; } = new();

        /// <summary>Kural 21: yalnız projede gerçekten var olan hedefler listelenir (sahte modül yok).
        /// K4: her öğe gereken izni taşır; yetkisiz olan `YetkileriUygulaAsync`'ta görünür listeden çıkarılır.</summary>
        public void InitializeNavigationItems()
        {
            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem(0xE80F, "Genel Bakış", typeof(DashboardViewModel)),
                new NavigationItem(0xE716, "Firma Yönetimi", typeof(FirmalarViewModel), Permission.Firma_Yonet),
                new NavigationItem(0xE7C3, "Sistem Kayıtları", typeof(SistemLogsViewModel), Permission.Log_Goruntule)
            };
        }

        /// <summary>K4: menü öğelerini kullanıcının izinlerine göre süzer; yetkisiz öğe gizlenir.</summary>
        public async Task YetkileriUygulaAsync()
        {
            GorunurNavigationItems.Clear();
            if (_yetki == null || NavigationItems == null)
            {
                foreach (var oge in NavigationItems ?? Enumerable.Empty<NavigationItem>())
                    GorunurNavigationItems.Add(oge);
                return;
            }

            foreach (var item in NavigationItems)
            {
                if (item.GerekliIzin is Permission izin && !await _yetki.KullaniciYetkisiVarMiAsync(izin))
                    continue;
                GorunurNavigationItems.Add(item);
            }
        }
    }
}
