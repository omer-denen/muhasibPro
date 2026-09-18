using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi
{
    /// <summary>Bir modülün (kategori) izinleri. `TumuSecili` master anahtarıdır:
    /// işaretlenince tüm aksiyonlar seçilir; tüm aksiyonlar seçilince otomatik işaretlenir.
    /// `SayiOzeti` satırda "seçili/toplam" göstergesi için kullanılır.</summary>
    public class ModulYetki : INotifyPropertyChanged
    {
        public ModulYetki(string kategori) => Kategori = kategori;

        public string Kategori { get; }

        public ObservableCollection<RolIzinModel> Izinler { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _tumuSecili;
        public bool TumuSecili
        {
            get => _tumuSecili;
            set
            {
                if (_tumuSecili == value) return;
                _tumuSecili = value;
                foreach (var izin in Izinler)
                    izin.Secili = value;
                Raise(nameof(TumuSecili));
                RaiseOzet();
            }
        }

        public int Toplam => Izinler.Count;
        public int Secilen => Izinler.Count(i => i.Secili);

        /// <summary>Satır göstergesi (ör. "3/4").</summary>
        public string SayiOzeti => $"{Secilen}/{Toplam}";

        /// <summary>Tek aksiyon değişince çağrılır; hepsi seçiliyse `TumuSecili` işaretlenir.</summary>
        public void AksiyonSenkronize()
        {
            bool hepsi = Izinler.Count > 0 && Izinler.All(i => i.Secili);
            if (_tumuSecili != hepsi)
            {
                _tumuSecili = hepsi;
                Raise(nameof(TumuSecili));
            }

            RaiseOzet();
        }

        private void RaiseOzet()
        {
            Raise(nameof(Secilen));
            Raise(nameof(SayiOzeti));
        }

        private void Raise(string ad) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ad));
    }

    /// <summary>Faz 6.85 K3: kullanıcı bazlı izin yönetimi — solda modül listesi, sağda seçili modülün
    /// aksiyon kartı ve üstte rolün izin özeti + "Kullanıcı için ata". İzinler kullanıcının rolüne
    /// aittir (aynı roldeki tüm kullanıcıları etkiler); `Yönetici` rolü salt-okunurdur.</summary>
    public class KullaniciRolYetkiViewModel : ViewModelBase
    {
        private readonly IRolYetkiService _rolYetkiService;
        private readonly IKullaniciService _kullaniciService;
        private Dictionary<Permission, bool> _orijinal = new();

        public KullaniciRolYetkiViewModel(
            ICommonServices commonServices,
            IRolYetkiService rolYetkiService,
            IKullaniciService kullaniciService) : base(commonServices)
        {
            _rolYetkiService = rolYetkiService;
            _kullaniciService = kullaniciService;
        }

        public ObservableCollection<KullaniciModel> Kullanicilar { get; } = new();
        public ObservableCollection<ModulYetki> Moduller { get; } = new();

        private KullaniciModel _seciliKullanici;
        public KullaniciModel SeciliKullanici
        {
            get => _seciliKullanici;
            set
            {
                if (Set(ref _seciliKullanici, value))
                {
                    NotifyPropertyChanged(nameof(SeciliRolAdi));
                    NotifyPropertyChanged(nameof(YoneticiMi));
                    NotifyPropertyChanged(nameof(Duzenlenebilir));
                    NotifyPropertyChanged(nameof(RolBilgi));
                    _ = MatrisYukleAsync();
                }
            }
        }

        private ModulYetki _seciliModul;
        public ModulYetki SeciliModul
        {
            get => _seciliModul;
            set { if (Set(ref _seciliModul, value)) NotifyPropertyChanged(nameof(ModulVar)); }
        }

        public bool ModulVar => _seciliModul != null;

        public bool KullaniciVar => Kullanicilar.Count > 0;
        public string SeciliRolAdi => _seciliKullanici?.Rol?.RolAdi ?? "-";
        public bool YoneticiMi => _seciliKullanici?.Rol?.RolTip == KullaniciRolTip.Yönetici;
        public bool Duzenlenebilir => _seciliKullanici != null && !YoneticiMi;

        public string RolBilgi => _seciliKullanici == null
            ? "Kullanıcı seçin."
            : YoneticiMi
                ? $"Seçili kullanıcı '{SeciliRolAdi}' rolünde — tüm izinlere sahiptir, değiştirilemez."
                : $"Seçili kullanıcının rolü: '{SeciliRolAdi}'. Değişiklikler 'Kullanıcı için ata' ile aynı roldeki tüm kullanıcılara uygulanır.";

        private int _atananSayi;
        public int AtananSayi { get => _atananSayi; private set => Set(ref _atananSayi, value); }

        private int _toplamIzinSayisi;
        public int ToplamIzinSayisi { get => _toplamIzinSayisi; private set => Set(ref _toplamIzinSayisi, value); }

        private int _degisiklikSayisi;
        public int DegisiklikSayisi { get => _degisiklikSayisi; private set => Set(ref _degisiklikSayisi, value); }

        public bool DegisiklikVar => _degisiklikSayisi > 0;

        /// <summary>Üst şeritteki düz-dil özet (ör. "34 / 71 izin").</summary>
        public string AtananOzet => $"{AtananSayi} / {ToplamIzinSayisi} izin";

        /// <summary>Atanan izin oranı (0-100); ilerleme çubuğu için.</summary>
        public double IlerlemeYuzdesi => ToplamIzinSayisi > 0
            ? 100.0 * AtananSayi / ToplamIzinSayisi
            : 0;

        /// <summary>Kaydetmeden önce fark bilgisi (Kural 12/16 — onay öncesi fark görünür olmalı).</summary>
        public string DegisiklikOzeti => _degisiklikSayisi == 0
            ? "Değişiklik yok"
            : $"{_degisiklikSayisi} değişiklik var";

        private bool _isYukleniyor;
        public bool IsYukleniyor { get => _isYukleniyor; set => Set(ref _isYukleniyor, value); }

        private bool _isKaydediliyor;
        public bool IsKaydediliyor { get => _isKaydediliyor; set => Set(ref _isKaydediliyor, value); }

        public ICommand AtaCommand => new AsyncRelayCommand(AtayaAsync);

        /// <summary>Hazır ayar presetleri: "TumunuVer" · "YalnizGoruntule" · "Temizle".
        /// Bulk işlem staged'dir; "Kullanıcı için ata" ile uygulanır.</summary>
        public ICommand HazirAyarCommand => new RelayCommand<string>(HazirAyarUygula);

        public async Task LoadAsync(long firmaId)
        {
            IsYukleniyor = true;
            try
            {
                var sonuc = await _kullaniciService.GetKullanicilarWithRolAsync(firmaId);
                Kullanicilar.Clear();
                if (sonuc.Success && sonuc.Data != null)
                    foreach (var kullanici in sonuc.Data)
                        Kullanicilar.Add(kullanici);
                NotifyPropertyChanged(nameof(KullaniciVar));

                if (Kullanicilar.Count > 0)
                    SeciliKullanici = Kullanicilar[0];
                else
                    Bosalt();
            }
            catch (Exception ex)
            {
                StatusError("Kullanıcı listesi yüklenirken beklenmeyen hata");
                await LogSistemExceptionAsync("Roller & İzinler", "Kullanıcılar", ex);
            }
            finally
            {
                IsYukleniyor = false;
            }
        }

        private async Task MatrisYukleAsync()
        {
            var rolId = _seciliKullanici?.RolId ?? 0;
            if (rolId <= 0)
            {
                Bosalt();
                return;
            }

            IsYukleniyor = true;
            try
            {
                var sonuc = await _rolYetkiService.GetMatrisAsync(rolId);
                if (!sonuc.Success || sonuc.Data == null)
                {
                    Bosalt();
                    StatusError(sonuc.Message);
                    NotificationService.Show("Hata", sonuc.Message, NotificationType.Danger);
                    return;
                }

                Moduller.Clear();
                _orijinal = sonuc.Data.ToDictionary(i => i.Izin, i => i.Secili);

                foreach (var grup in sonuc.Data.GroupBy(i => i.Kategori).OrderBy(g => g.Key))
                {
                    var modul = new ModulYetki(grup.Key);
                    foreach (var izin in grup.OrderBy(i => i.Ad))
                    {
                        ((INotifyPropertyChanged)izin).PropertyChanged += OnIzinPropertyChanged;
                        modul.Izinler.Add(izin);
                    }
                    modul.AksiyonSenkronize();
                    Moduller.Add(modul);
                }

                SeciliModul = Moduller.FirstOrDefault();
                GuncelleOzetler();
            }
            catch (Exception ex)
            {
                StatusError("İzinler yüklenirken beklenmeyen hata");
                await LogSistemExceptionAsync("Roller & İzinler", "Yükle", ex);
            }
            finally
            {
                IsYukleniyor = false;
            }
        }

        private void OnIzinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(RolIzinModel.Secili)) return;
            foreach (var modul in Moduller)
            {
                if (modul.Izinler.Contains(sender))
                {
                    modul.AksiyonSenkronize();
                    break;
                }
            }
            GuncelleOzetler();
        }

        /// <summary>Staged (değişmiş) izinleri role uygular.</summary>
        private async Task AtayaAsync()
        {
            var rolId = _seciliKullanici?.RolId ?? 0;
            if (rolId <= 0 || !Duzenlenebilir)
                return;

            var simdi = Moduller.SelectMany(m => m.Izinler).ToDictionary(i => i.Izin, i => i.Secili);
            var degisenler = simdi.Where(kv => !_orijinal.TryGetValue(kv.Key, out var o) || o != kv.Value).ToList();

            if (degisenler.Count == 0)
            {
                StatusActionMessage("Değişiklik yok", StatusMessageType.Success, autoHide: 3);
                return;
            }

            IsKaydediliyor = true;
            try
            {
                foreach (var kv in degisenler)
                {
                    var sonuc = await _rolYetkiService.SetIzinAsync(rolId, kv.Key, kv.Value);
                    if (!sonuc.Success)
                    {
                        StatusError(sonuc.Message);
                        NotificationService.Show("Hata", sonuc.Message, NotificationType.Danger);
                        return;
                    }
                    _orijinal[kv.Key] = kv.Value;
                }

                GuncelleOzetler();
                StatusActionMessage($"{degisenler.Count} izin güncellendi", StatusMessageType.Success, autoHide: 3);
                NotificationService.Show("Başarılı", $"{degisenler.Count} izin kullanıcının rolüne atandı.", NotificationType.Success);
            }
            catch (Exception ex)
            {
                StatusError("İzinler atanırken beklenmeyen hata");
                await LogSistemExceptionAsync("Roller & İzinler", "Ata", ex);
            }
            finally
            {
                IsKaydediliyor = false;
            }
        }

        private void HazirAyarUygula(string ayar)
        {
            if (!Duzenlenebilir)
                return;

            foreach (var modul in Moduller)
            {
                foreach (var izin in modul.Izinler)
                {
                    izin.Secili = ayar switch
                    {
                        "TumunuVer" => true,
                        "YalnizGoruntule" => izin.Izin.ToString().EndsWith("_Goruntule", StringComparison.Ordinal),
                        _ => false
                    };
                }
                modul.AksiyonSenkronize();
            }

            GuncelleOzetler();
        }

        private void GuncelleOzetler()
        {
            AtananSayi = Moduller.Sum(m => m.Secilen);
            ToplamIzinSayisi = Moduller.Sum(m => m.Toplam);
            DegisiklikSayisi = Moduller.SelectMany(m => m.Izinler)
                .Count(i => !_orijinal.TryGetValue(i.Izin, out var o) || o != i.Secili);
            NotifyPropertyChanged(nameof(DegisiklikVar));
            NotifyPropertyChanged(nameof(DegisiklikOzeti));
            NotifyPropertyChanged(nameof(AtananOzet));
            NotifyPropertyChanged(nameof(IlerlemeYuzdesi));
        }

        private void Bosalt()
        {
            Moduller.Clear();
            SeciliModul = null;
            _orijinal = new Dictionary<Permission, bool>();
            GuncelleOzetler();
        }
    }
}
