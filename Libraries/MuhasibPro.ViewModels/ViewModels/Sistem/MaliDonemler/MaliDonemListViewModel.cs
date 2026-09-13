using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Linq.Expressions;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler
{
    public class MaliDonemListArgs
    {
        public static MaliDonemListArgs CreateEmpty() => new() { IsEmpty = true };

        public MaliDonemListArgs() { OrderBy = r => r.MaliYil; }

        public bool IsEmpty { get; set; }

        public string Query { get; set; }

        public long FirmaId { get; set; }

        public Expression<Func<MaliDonem, object>> OrderBy { get; set; }

        public Expression<Func<MaliDonem, object>> OrderByDesc { get; set; }

        public Expression<Func<MaliDonem, object>>[] Includes { get; set; }
    }

    public class MaliDonemListViewModel : GenericListViewModel<MaliDonemModel>
    {
        public MaliDonemListViewModel(
            ICommonServices commonServices,
            IMaliDonemService maliDonemService,
            ITenantSQLiteDatabaseService tenantDatabaseService = null,
            IEventBus eventBus = null) : base(commonServices)
        {
            MaliDonemService = maliDonemService;
            TenantDatabaseService = tenantDatabaseService;
            EventBus = eventBus;
        }

        /// <summary>Toplu analiz tamamlandığında tetiklenir (KPI tazeleme için).</summary>
        public event Action TopluAnalizTamamlandi;

        /// <summary>Verilirse E1 güncelleme olayında kart rozeti refresh'siz güncellenir.</summary>
        public IEventBus EventBus { get; }

        public IMaliDonemService MaliDonemService { get; }

        /// <summary>Verilirse, dönem listesi yüklenirken her dönemin tenant DB'si analiz edilir (kart "DB Durumu" rozeti).</summary>
        public ITenantSQLiteDatabaseService TenantDatabaseService { get; }

        /// <summary>
        /// Paylaşılan Scoped SistemDbContext'e erişim kapısı. Refresh (okuma) ile backfill
        /// (yazım) üst üste binerse EF "second operation started" fırlatır (EventId 10100) —
        /// Global.db dokunan tüm akışlar bu kapıdan serileşir. Tenant dosya analizi kendi
        /// bağlantısını açtığı için kapı dışındadır (UI bloklanmaz).
        /// </summary>
        private readonly SemaphoreSlim _dbGate = new(1, 1);

        private string Header => "Mali Dönem";

        private bool _isListeYukleniyor;
        /// <summary>Liste Global.db'den okunurken kart ring gösterir (Kural 11: try/finally ile kapanır).</summary>
        public bool IsListeYukleniyor
        {
            get => _isListeYukleniyor;
            private set => Set(ref _isListeYukleniyor, value);
        }

        public MaliDonemListArgs ViewModelArgs { get; private set; }

    public async Task LoadAsync(MaliDonemListArgs args, bool silent = false)
    {
        ViewModelArgs = args ?? MaliDonemListArgs.CreateEmpty();
        Query = ViewModelArgs.Query;
        if(silent)
        {
            await RefreshAsync();
        } else
        {
            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} listesi yükleniyor....",
                startMessageType: StatusMessageType.Refreshing,
                successMessage: $"{Header} listesi yüklendi");
        }
    }



        public void Unload() 
        {
            if(ViewModelArgs != null)
                ViewModelArgs.Query = Query;
        }

        public void Subscribe()
        {
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnMessage);
            MessageService.Subscribe<MaliDonemDetailsViewModel>(this, OnMessage);
            EventBus?.Subscribe<TenantUpdateAvailableEvent>(this, OnTenantUpdateAvailable);
        }

        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
            EventBus?.Unsubscribe(this);
        }

        /// <summary>E1: güncelleme saptanan dönemin rozeti analiz-refresh'siz "Güncelleme Gerekli" olur.</summary>
        private async void OnTenantUpdateAvailable(object sender, TenantUpdateAvailableEvent e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.DatabaseName))
                return;
            var model = ItemsSource?.FirstOrDefault(m => m != null
                && string.Equals(m.DatabaseName, e.DatabaseName, StringComparison.OrdinalIgnoreCase));
            if (model == null)
                return;
            await ContextService.RunAsync(() =>
            {
                model.DbAnalizYapildi = true;
                model.DbDurum = Domain.Enum.DatabaseEnum.DatabaseStatusResult.RequiredUpdating;
                model.DbAnalizDetay = $"Güncelleme gerekli: {e.FromVersion} → {e.ToVersion}.";
            });
        }

        public MaliDonemListArgs CreateArgs()
        {
            return new MaliDonemListArgs
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc,
                Includes = ViewModelArgs.Includes,
                FirmaId = ViewModelArgs.FirmaId,
            };
        }

        public async Task<bool> RefreshAsync()
        {
            // Kapı bilerek ConfigureAwait'siz — devam eden NotifyPropertyChanged UI thread'inde koşar.
            IsListeYukleniyor = true;
            await _dbGate.WaitAsync();
            try
            {
                await LoadDataAsync();
                NotifyPropertyChanged(nameof(Title));  // ✅ SADECE BAŞARILI DURUMDA
                return true;
            }  catch(Exception ex)
            {
                StatusError($"{Header} listesi yenilenirken beklenmeyen hata");
                await LogSistemExceptionAsync($"{Header}", "Yenile", ex);
                return false;
            }
            finally
            {
                _dbGate.Release();
                IsListeYukleniyor = false;
            }
        }

        protected async override Task LoadDataAsync()
        {
            if(ViewModelArgs == null || ViewModelArgs.IsEmpty)
            {
                Items = new List<MaliDonemModel>();      // ← ITEMS'I TEMIZLE
                await ContextService.RunAsync(
                    () =>
                    {
                        ItemsSource?.Clear();
                    });
                ItemsCount = 0;
                SelectedItem = null;
            } else
            {
                DataRequest<MaliDonem> request = BuildDataRequest();
                var count = await MaliDonemService.GetMaliDonemlerCountAsync(request);
                ItemsCount = count?.Data ?? 0;
                var items = await MaliDonemService.GetMaliDonemlerWithFirmaId(request,firmaId:ViewModelArgs.FirmaId);
                Items = items?.Data;
                // B1-A: firma seçiminde TÜM dönemler paralel analizlenir (Oturum 72 deseni:
                // Task.WhenAll + per-item try/catch AnalyzeDbStatusAsync içinde). Seçili
                // kart + ProgressRing yanında seçili-olmayan kartlar da rozetini alır.
                if (Items != null)
                {
                    await ContextService.RunAsync(
                        () =>
                        {
                            long preservedId = SelectedItem?.Id ?? 0;
                            SelectedItem = null;
                            ItemsSource.Clear();
                            foreach (var item in Items)
                            {
                                ItemsSource.Add(item);
                            }
                            if (!IsMultipleSelection && ItemsSource.Count > 0)
                            {
                                MaliDonemModel pick = null;
                                if (preservedId > 0)
                                    pick = ItemsSource.FirstOrDefault(i => i.Id == preservedId);
                                // Kapalı ise ilk açık olana düş (varsayılan ilk kapalı ise bildirim çıkmasın)
                                if (pick != null && pick.KapaliMi)
                                    pick = ItemsSource.FirstOrDefault(i => !i.KapaliMi) ?? pick;
                                if (pick == null)
                                    pick = ItemsSource.FirstOrDefault(i => !i.KapaliMi) ?? ItemsSource.First();
                                SelectedItem = pick;
                            }
                        });
                    // Tüm kartlar paralel analizlenir (seçili + seçili-olmayan). Fire-and-forget:
                    // liste akışı bloklanmaz; tenant analizi kendi bağlantısını açar, içindeki
                    // Global.db backfill'i _dbGate'ten geçer, kapı tutuluyorsa kuyrukta bekler.
                    _ = AnalyzeAllDbStatusesAsync(ItemsSource.Where(m => m != null).ToList());
                }
            }
        }

        /// <summary>
        /// Listedeki TÜM dönemlerin tenant DB'sini paralel analiz eder (B1-A).
        /// AnalyzeDbStatusAsync exception-safe'dir (içi try/catch) — tek kartın arızası
        /// listeyi kırmaz; WhenAll yine de savunma amaçlı try/catch ile sarılıdır.
        /// </summary>
        public async Task AnalyzeAllDbStatusesAsync(IReadOnlyList<MaliDonemModel> items)
        {
            if (items == null || items.Count == 0 || TenantDatabaseService == null)
                return;
            try
            {
                await Task.WhenAll(items.Where(m => m != null).Select(m => AnalyzeDbStatusAsync(m))).ConfigureAwait(false);
            }
            catch
            {
                // Savunma: AnalyzeDbStatusAsync fırlatmaz; liste akışı yine de kırılmaz.
            }
            TopluAnalizTamamlandi?.Invoke();
        }

        /// <summary>
        /// Seçili kartın tenant DB'sini analiz eder; kart içi IsDbAnalyzing + ProgressRing gösterir.
        /// Timeout 20sn — takılırsa overlay kapanır ve rozet güncellenir.
        /// Sonuç modelin DbAnalizYapildi/DbDurum alanlarına yazılır (DatabaseStatusShortTextConverter kısa metin).
        /// </summary>
        public async Task AnalyzeDbStatusAsync(MaliDonemModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.DatabaseName) || TenantDatabaseService == null)
                return;
            if (model.IsDbAnalyzing)
                return;
            await ContextService.RunAsync(() => model.IsDbAnalyzing = true);
            try
            {
                var task = TenantDatabaseService.GetTenantDatabaseStateAsync(model.DatabaseName);
                var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(20))).ConfigureAwait(false);
                if (completed != task)
                {
                    await ContextService.RunAsync(() =>
                    {
                        model.DbAnalizYapildi = true;
                        model.DbDurum = Domain.Enum.DatabaseEnum.DatabaseStatusResult.UnknownError;
                        model.DbAnalizDetay = "Analiz zaman aşımına uğradı (20sn). Veritabanı dosyası kilitli olabilir.";
                    });
                    return;
                }
                var response = await task.ConfigureAwait(false);
                var analysis = response?.Data;
                if (analysis == null)
                {
                    await ContextService.RunAsync(() =>
                    {
                        model.DbAnalizYapildi = true;
                        model.DbDurum = Domain.Enum.DatabaseEnum.DatabaseStatusResult.UnknownError;
                        model.DbAnalizDetay = response?.Message ?? "Analiz sonucu alınamadı.";
                    });
                    return;
                }
                await ContextService.RunAsync(() =>
                {
                    model.DbAnalizYapildi = true;
                    model.DbAnaliz = analysis;
                    model.DbDurum = analysis.GetStatus();
                    model.DbBekleyenGuncellemeSayisi = analysis.PendingMigrations?.Count ?? 0;
                    model.DbAnalizDetay = analysis.GetStatusMessage() ?? model.DbDurum.ToString();
                    // Analiz dosya boyutunu da taşır — kart vitrinine işle (BoyutMetni buradan okur).
                    model.TenantDetails ??= new TenantDetailsModel();
                    model.TenantDetails.DosyaBoyutu = analysis.DatabaseFileSizeBytes;
                    model.RefreshVitrin();
                });
                // Satır backfill: eski satırlarda DosyaBoyutu NULL olabilir — analiz gerçeğini
                // Global.db satırına tek seferlik işle (sonraki koşul tutmaz, yakınsar).
                await BackfillFileSizeAsync(model, analysis.DatabaseFileSizeBytes);
            }
            catch (Exception ex)
            {
                await ContextService.RunAsync(() =>
                {
                    model.DbAnalizYapildi = true;
                    model.DbDurum = Domain.Enum.DatabaseEnum.DatabaseStatusResult.UnknownError;
                    model.DbAnalizDetay = ex.Message;
                });
            }
            finally
            {
                await ContextService.RunAsync(() => model.IsDbAnalyzing = false);
            }
        }

        /// <summary>
        /// Global.db satırındaki DosyaBoyutu NULL/0 ise analizden gelen gerçek boyutu satıra işler.
        /// Eski satırların (Oturum 71 öncesi) kartında boyut dolsun diye — koşul tutmayınca yazım durur.
        /// Refresh ile aynı kapıdan geçer (paylaşılan Scoped context çakışmasın).
        /// </summary>
        private async Task BackfillFileSizeAsync(MaliDonemModel model, long analyzedBytes)
        {
            try
            {
                if (model == null || analyzedBytes <= 0 || MaliDonemService == null)
                    return;
                await _dbGate.WaitAsync();
                try
                {
                    var current = await MaliDonemService.GetByMaliDonemIdAsync(model.Id);
                    var row = current?.Data;
                    if (row?.TenantDetails == null || (row.TenantDetails.DosyaBoyutu ?? 0) > 0)
                        return;
                    row.TenantDetails.DosyaBoyutu = analyzedBytes;
                    await MaliDonemService.UpdateMaliDonemAsync(row);
                }
                finally
                {
                    _dbGate.Release();
                }
            }
            catch
            {
                // Backfill best-effort — kart zaten bellek içi değeri gösteriyor, liste kırılmaz.
            }
        }

        private DataRequest<MaliDonem> BuildDataRequest()
        {
            var request = new DataRequest<MaliDonem>()
            {
                Query = Query,
                OrderBy = ViewModelArgs.OrderBy,
                OrderByDesc = ViewModelArgs.OrderByDesc,
                Includes = ViewModelArgs.Includes
            };
            if(ViewModelArgs.FirmaId > 0)
            {
                request.Where = (r) => r.FirmaId == ViewModelArgs.FirmaId;
            }
            return request;
        }

        public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);

        private async void OnOpenInNewView()
        {
            if(SelectedItem != null)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { MaliDonemId = SelectedItem.Id },
                    customTitle: $"{Header}'ler");
            }
        }

        protected override async void OnNew()
        {
            if(IsMainWindow)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { FirmaId = ViewModelArgs.FirmaId },
                    customTitle: $"Yeni {Header}");
            } else
            {
                NavigationService.Navigate<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { FirmaId = ViewModelArgs.FirmaId });
            }
            StatusReady();
        }

        protected async override void OnDeleteSelection()
        {
            StatusReady();
            await DialogService.ShowInfoAsync(
                "Bilgilendirme",
                "Bu işlem kritik! Çoklu silme tarafında bu işlem yapılamaz.");

            await RefreshAsync();
            SelectedIndexRanges = null;
            SelectedItems = null;
            StopProgress();
        }

        protected async override void OnRefresh()
        {
            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} listesi yenileniyor...",
                startMessageType: StatusMessageType.Refreshing,
                successMessage: $"{Header} listesi yenilendi");
        }

        private async void OnMessage(ViewModelBase sender, string message, object args)
        {
            switch(message)
            {
                case EntityEvents.NewItemSaved:
                case EntityEvents.ItemDeleted:
                case EntityEvents.ItemsDeleted:
                case EntityEvents.ItemRangesDeleted:
                    await ContextService.RunAsync(
                        async () =>
                        {
                            await RefreshAsync();
                        });
                    break;
            }
        }
    }
}
