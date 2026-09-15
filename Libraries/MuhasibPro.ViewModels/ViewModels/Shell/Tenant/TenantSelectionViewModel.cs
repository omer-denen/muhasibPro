using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Sistem.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>
    /// FirmaShell seçim durumu + kalıcılık + giriş guard'ları. Navigation/dialog sadece bu sınıfın işi.
    /// </summary>
    public class TenantSelectionViewModel : ViewModelBase
    {
        public TenantSelectionViewModel(
            MuhasibPro.Business.Contracts.UIServices.CommonServices.ICommonServices commonServices,
            MuhasibPro.Business.Contracts.UIServices.ILocalSettingsService localSettingsService) : base(commonServices)
        {
            LocalSettingsService = localSettingsService;
        }

        public MuhasibPro.Business.Contracts.UIServices.ILocalSettingsService LocalSettingsService { get; }

        /// <summary>Yerel ayarlarda kayıtlı son dönem seçimi (rozet için; LoadLastSelectionAsync doldurur).</summary>
        public long SonKayitliDonemId { get; private set; }

        private FirmaModel _selectedFirma;
        public FirmaModel SelectedFirma
        {
            get => _selectedFirma;
            private set
            {
                if (Set(ref _selectedFirma, value))
                {
                    NotifyPropertyChanged(nameof(IsFirmaSelected));
                    NotifyPropertyChanged(nameof(HasSelection));
                }
            }
        }

        private MaliDonemModel _selectedMaliDonem;
        public MaliDonemModel SelectedMaliDonem
        {
            get => _selectedMaliDonem;
            private set
            {
                if (Set(ref _selectedMaliDonem, value))
                {
                    NotifyPropertyChanged(nameof(IsMaliDonemSelected));
                    NotifyPropertyChanged(nameof(HasSelection));
                }
            }
        }

        public bool IsFirmaSelected => SelectedFirma != null;
        public bool IsMaliDonemSelected => SelectedMaliDonem != null;
        public bool HasSelection => IsFirmaSelected && IsMaliDonemSelected;

        public void SelectFirma(FirmaModel firma) => SelectedFirma = firma;
        public void SelectMaliDonem(MaliDonemModel donem) => SelectedMaliDonem = donem;

        /// <summary>Çalışma alanına giriş guard'ları (kapalı dönem / dosya yok).</summary>
        public WorkspaceEntryGuard CheckWorkspaceEntry()
        {
            if (SelectedMaliDonem != null && SelectedMaliDonem.KapaliMi)
                return WorkspaceEntryGuard.Deny("Kapalı Dönem", $"{SelectedMaliDonem.MaliYil} dönemi kapalı olduğu için çalışma alanına giriş yapılamaz. Lütfen açık bir dönem seçin.");
            if (SelectedMaliDonem != null && SelectedMaliDonem.DbDosyaYokMu)
                return WorkspaceEntryGuard.Deny("Veritabanı Bulunamadı", $"{SelectedMaliDonem.MaliYil} döneminin veritabanı dosyası diskte bulunamadı. Bu döneme giriş yapılamaz — kaydı dönem kartındaki Sil ile temizleyebilirsiniz.");
            return WorkspaceEntryGuard.Permit();
        }

        public async Task EnsureFirmaExistsAsync()
        {
            var result = await DialogService.ShowAsync(
                "Firma Bulunamadı",
                "Henüz kayıtlı firma bulunmuyor. Yeni firma eklemek ister misiniz?",
                "Evet",
                "Hayır");

            if (result)
                await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs());
        }

        public async Task LoadLastSelectionAsync(FirmaListViewModel firmaList, MaliDonemListViewModel maliDonemList)
        {
            try
            {
                var lastFirmaId = await LocalSettingsService.ReadSettingAsync<long>("LastSelectedFirmaId");
                if (lastFirmaId <= 0)
                    return;

                var lastFirma = firmaList.Items.FirstOrDefault(f => f.Id == lastFirmaId);
                if (lastFirma == null)
                    return;

                firmaList.SelectedItem = lastFirma;
                // SelectedFirma'yı doğrudan set et — mesaj dispatch'i beklemeden
                SelectedFirma = lastFirma;

                // Mali dönem yüklemesi için bekle (FirmalarVM.OnItemSelected async void olduğu için)
                await Task.Delay(200);

                var lastDonemId = await LocalSettingsService.ReadSettingAsync<long>("LastSelectedDonemId");
                SonKayitliDonemId = lastDonemId;
                if (lastDonemId > 0)
                {
                    var lastDonem = maliDonemList.Items?.FirstOrDefault(d => d.Id == lastDonemId);
                    // Kapalı ise ilk açık olana düş
                    if (lastDonem != null && lastDonem.KapaliMi)
                        lastDonem = maliDonemList.Items?.FirstOrDefault(d => !d.KapaliMi) ?? lastDonem;
                    if (lastDonem != null)
                    {
                        maliDonemList.SelectedItem = lastDonem;
                        SelectedMaliDonem = lastDonem;
                    }
                }
                else if (maliDonemList.SelectedItem != null && maliDonemList.SelectedItem.KapaliMi)
                {
                    // Hiç kayıtlı seçim yoksa ve ilk seçili kapalı ise açık olana geç
                    var firstOpen = maliDonemList.Items?.FirstOrDefault(d => !d.KapaliMi);
                    if (firstOpen != null)
                    {
                        maliDonemList.SelectedItem = firstOpen;
                        SelectedMaliDonem = firstOpen;
                    }
                }
            }
            catch (Exception ex)
            {
                await LogSistemExceptionAsync("FirmaDonemSelect", "LoadLastSelection", ex);
            }
        }

        public async Task SaveLastSelectionAsync()
        {
            try
            {
                await LocalSettingsService.SaveSettingAsync("LastSelectedFirmaId", SelectedFirma.Id);
                await LocalSettingsService.SaveSettingAsync("LastSelectedDonemId", SelectedMaliDonem.Id);
            }
            catch (Exception ex)
            {
                // Kaydetme hatası kritik değil, sessizce devam et
                await LogSistemExceptionAsync("FirmaDonemSelect", "SaveLastSelection", ex);
            }
        }
    }

    public sealed class WorkspaceEntryGuard
    {
        public bool IsAllowed { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;

        public static WorkspaceEntryGuard Permit() => new() { IsAllowed = true };
        public static WorkspaceEntryGuard Deny(string title, string message) => new() { Title = title, Message = message };
    }
}
