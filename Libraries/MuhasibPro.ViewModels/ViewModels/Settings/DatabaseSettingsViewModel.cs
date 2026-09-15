using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.Domain.Models;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Settings
{
    public class DatabaseSettingsViewModel : ViewModelBase
    {
        private readonly ILocalSettingsService _localSettings;
        private readonly IAuthenticationService _auth;
        private readonly IEventBus _eventBus;

        public DatabaseSettingsViewModel(
            ICommonServices commonServices,
            ILocalSettingsService localSettings,
            IAuthenticationService auth = null!,
            IEventBus eventBus = null!) : base(commonServices)
        {
            _localSettings = localSettings;
            _auth = auth;
            _eventBus = eventBus;
            _settings = new DatabaseSettingsModel();
        }

        private DatabaseSettingsModel _settings;
        public DatabaseSettingsModel Settings
        {
            get => _settings;
            set => Set(ref _settings, value);
        }

        private int _maxYedekSayisi = 5;
        public int MaxManuelYedekSayisi
        {
            get => _maxYedekSayisi;
            set
            {
                var clamped = Math.Clamp(value, 1, 20);
                if (Set(ref _maxYedekSayisi, clamped))
                {
                    Settings.MaxManuelYedekSayisi = clamped;
                    _ = SaveAsync();
                }
            }
        }

        private bool _otomatikTemizleme = true;
        public bool OtomatikTemizlemeAcik
        {
            get => _otomatikTemizleme;
            set
            {
                if (Set(ref _otomatikTemizleme, value))
                {
                    Settings.OtomatikTemizlemeAcik = value;
                    _ = SaveAsync();
                }
            }
        }

        private bool _kapanistaYedek = false;
        public bool KapanistaOtomatikYedek
        {
            get => _kapanistaYedek;
            set
            {
                if (Set(ref _kapanistaYedek, value))
                {
                    Settings.KapanistaOtomatikYedek = value;
                    _ = SaveAsync();
                }
            }
        }

        private bool _haftalikKontrol = true;
        public bool HaftalikButunlukKontrolu
        {
            get => _haftalikKontrol;
            set
            {
                if (Set(ref _haftalikKontrol, value))
                {
                    Settings.HaftalikButunlukKontrolu = value;
                    _ = SaveAsync();
                }
            }
        }

        public string YedekKlasoruYolu => "%LocalAppData%\\MuhasibPro\\Yedekler (DEBUG: Databases\\Yedekler)";

        private System.Windows.Input.ICommand _yardimCommand;

        /// <summary>Kural 13: sayfa yardımı (içerik ViewModel'de, dialog chrome'u App'te).</summary>
        public System.Windows.Input.ICommand YardimCommand =>
            _yardimCommand ??= new Infrastructure.Common.AsyncRelayCommand(YardimGoster);

        private async Task YardimGoster()
        {
            await DialogService.ShowYardimAsync("Veritabanı Ayarları — Yardım", new List<Business.DTOModel.SistemModel.YardimMaddesiDto>
            {
                new() { Baslik = "Manuel yedek saklama limiti", Aciklama = "Her dönem için saklanacak en fazla manuel yedek sayısıdır (1-20). Limit aşılınca en eski yedek otomatik silinir (FIFO)." },
                new() { Baslik = "Otomatik temizleme", Aciklama = "Kapalıysa limit aşılsa bile yedekler silinmez; temizliği elle yaparsınız." },
                new() { Baslik = "Kapanışta otomatik yedek", Aciklama = "Uygulama kapatılırken açık dönemin ve Sistem.db'nin yedeği alınır. Uzun kapanış istemiyorsanız kapalı tutun." },
                new() { Baslik = "Haftalık bütünlük hatırlatması", Aciklama = "PRAGMA integrity_check 7 günden eskiyse uyarı gösterilir; kontrolü erteleyebilirsiniz." },
                new() { Baslik = "Yedek klasörü", Aciklama = "Tüm manuel yedekler bu klasörde saklanır. Dosyaları elle silmeyin; uygulama üzerinden yönetin." },
            });
        }

        public async Task LoadAsync()
        {
            try
            {
                var saved = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey);
                if (saved != null)
                {
                    Settings = saved;
                    _maxYedekSayisi = Math.Clamp(saved.MaxManuelYedekSayisi, 1, 20);
                    _otomatikTemizleme = saved.OtomatikTemizlemeAcik;
                    _kapanistaYedek = saved.KapanistaOtomatikYedek;
                    _haftalikKontrol = saved.HaftalikButunlukKontrolu;
                    NotifyPropertyChanged(nameof(MaxManuelYedekSayisi));
                    NotifyPropertyChanged(nameof(OtomatikTemizlemeAcik));
                    NotifyPropertyChanged(nameof(KapanistaOtomatikYedek));
                    NotifyPropertyChanged(nameof(HaftalikButunlukKontrolu));
                    NotifyPropertyChanged(nameof(YedekKlasoruYolu));
                }
                else
                {
                    AyarVarsayilanlariniYukle();
                }
            }
            catch
            {
                AyarVarsayilanlariniYukle();
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                Settings.MaxManuelYedekSayisi = MaxManuelYedekSayisi;
                var kayitli = await _localSettings.ReadSettingAsync<DatabaseSettingsModel>(DatabaseSettingsModel.SettingsKey)
                    ?? new DatabaseSettingsModel();
                AyarYetkiDenetimi.KritikDegisiklikleriDogrula(Settings, kayitli, _auth);
                await _localSettings.SaveSettingAsync(DatabaseSettingsModel.SettingsKey, Settings);
                _eventBus?.Publish(this, new AppSettingsChangedEvent(DatabaseSettingsModel.SettingsKey));
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusError(ex.Message);
            }
            catch { }
        }

        /// <summary>Model varsayılanları (gerçek veri modelden — hardcoded tekrar yok).</summary>
        private void AyarVarsayilanlariniYukle()
        {
            Settings = new DatabaseSettingsModel();
            _maxYedekSayisi = Settings.GetManuelKeep();
            _otomatikTemizleme = Settings.OtomatikTemizlemeAcik;
            _kapanistaYedek = Settings.KapanistaOtomatikYedek;
            _haftalikKontrol = Settings.HaftalikButunlukKontrolu;
        }
    }
}
