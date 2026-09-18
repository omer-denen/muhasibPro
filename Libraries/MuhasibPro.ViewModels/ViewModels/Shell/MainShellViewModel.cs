using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.ViewModels.ViewModels.Loggings.SistemLogs;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class MainShellViewModel : MainMenuViewModel
    {
        public MainShellViewModel(
            IAuthenticationService authenticationService,
            ISistemDatabaseService sistemDatabaseService,
            ICommonServices commonServices,
            IPermissionService permissionService = null) : base(authenticationService, sistemDatabaseService, commonServices, permissionService)
        {
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }
        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, value);
        }

        private System.Windows.Input.ICommand _yardimCommand;

        /// <summary>Kural 13: sayfa yardımı (içerik ViewModel'de, dialog chrome'u App'te).</summary>
        public System.Windows.Input.ICommand YardimCommand =>
            _yardimCommand ??= new Infrastructure.Common.AsyncRelayCommand(YardimGoster);

        internal const string YardimAnahtari = "MainShell";
        internal const string YardimBasligi = "Çalışma Alanı — Yardım";

        /// <summary>Kural 13 içeriği (? yardım dialogu — AI bilgi tabanı artık `docs/yardim/*.md`).</summary>
        internal static List<MuhasibPro.Business.DTOModel.SistemModel.YardimMaddesiDto> YardimMaddeleri() => new()
        {
            new() { Baslik = "Bu ekran nedir?", Aciklama = "Girişten ve firma/dönem seçiminden sonra açılan ana çalışma alanıdır: solda modül menüsü, ortada seçili modülün içeriği, altta durum çubuğu yer alır." },
            new() { Baslik = "Sol menü", Aciklama = "Yalnız gerçek hedefler listelenir: Genel Bakış, Firma Yönetimi (Firma Yönetimi izni) ve Sistem Kayıtları (Log görüntüleme izni). Yetkiniz olmayan öğe menüde gizlenir; altta aktif kullanıcı ve 'Oturumu Kapat' bulunur." },
            new() { Baslik = "Durum çubuğu", Aciklama = "Pencerenin altındaki şerit solda anlık durumu ve süren işin ilerlemesini; sağda sistem veritabanı göstergesini, aktif firma ve mali dönemi, kullanıcı adını ve saati gösterir." },
            new() { Baslik = "Asistan düğmesi", Aciklama = "Durum çubuğunun en sağındaki 'Asistan' düğmesi AI yardım panelini açar; soru yazıp yanıt alırsınız." },
        };

        private async Task YardimGoster()
        {
            await DialogService.ShowYardimAsync(YardimBasligi, YardimMaddeleri());
        }

        public override async Task LoadAsync(ShellArgs args)
        {
            InitializeNavigationItems();
            await YetkileriUygulaAsync();
            // Ana pencere kabuğu: içerik VM'ye navigasyon yok; yalnız durum çubuğunu besle.
            ViewModelArgs = args;
            if (args?.UserInfo != null)
            {
                UserInfo = args.UserInfo;
                UserInfoyuStatusBaraYaz();
            }
            // Aktif firma/dönem bağlamı gerçek yüklü tenant'tan alınır (workspace girişi).
            StatusBarService.RefreshAktifBaglam();
            await SistemVeritabaniDurumunuYazAsync();
        }

        public override void Subscribe()
        {
            MessageService.Subscribe<ILogService, AppLog>(this, OnLogServiceMessage);
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        public override void Unload()
        {
            base.Unload();
        }

        
        public async void NavigateTo(Type viewModel)
        {
            StatusReady();
            if (viewModel != null)
            {
                switch (viewModel.Name)
                {
                    case "DashboardViewModel":
                        NavigationService.Navigate(viewModel);
                        break;
                    case "CustomersViewModel":
                        //NavigationService.Navigate(viewModel, new CustomerListArgs());
                        break;
                    case "OrdersViewModel":
                        //NavigationService.Navigate(viewModel, new OrderListArgs());
                        break;
                    case "FirmalarViewModel":
                        NavigationService.Navigate(viewModel, new FirmaListArgs());
                        break;
                    case "SistemLogsViewModel":
                        NavigationService.Navigate(viewModel, new SistemLogListArgs());
                        await LogService.SistemLogService.SistemLogMarkAllAsReadAsync();
                        //await UpdateAppLogBadge();
                        break;
                    case "SettingsViewModel":
                        NavigationService.Navigate(viewModel);
                        break;
                    default:
                        NavigationService.Navigate<DashboardViewModel>();
                        break;
                }
            }
            else
            {
                // Hata durumunda ana sayfaya yönlendir
                await DialogService.ShowAsync("Bilgi", "Henüz bu sayfalar hazırlanmadı");
            }
        }

        private async void OnLogServiceMessage(ILogService logService, string message, AppLog log)
        {
            if (message == "LogAdded")
            {
                await ContextService.RunAsync(async () =>
                {
                    await UpdateAppLogBadge();
                });
            }
        }

        private async Task UpdateAppLogBadge()
        {
            int count = await LogService.SistemLogService.GetSistemLogsCountAsync(new DataRequest<SistemLog> { Where = r => !r.IsRead });
            //AppLogsItem.Badge = count > 0 ? count.ToString() : null;
        }
    }


}


