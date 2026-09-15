using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.DevServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.DevModel;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Database.SistemDatabase;
using MuhasibPro.Data.Contracts.Database.TenantDatabase;
using MuhasibPro.Domain;

namespace MuhasibPro.Business.Services.SistemServices.DevServices
{
    /// <summary>
    /// Modül entegrasyon testleri (donanım POST benzeri). Her modül için: (1) DI'da kayıtlı mı/çözülebiliyor mu,
    /// (2) kritik akış salt-okunur çalışıyor mu. Sonuç "hangi modül kırık" sorusunu tek listede yanıtlar.
    /// </summary>
    public class ModulTestCalistirici : IModulTestCalistirici
    {
        private readonly IServiceProvider _services;

        public ModulTestCalistirici(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>Modül → DI arayüzü kataloğu (kayıt + çözüm testi).</summary>
        private static readonly (string Modul, string Ad, Type Servis)[] KayitliServisler =
        {
            ("Çekirdek", "IApplicationPaths", typeof(IApplicationPaths)),
            ("Çekirdek", "ILocalSettingsService", typeof(ILocalSettingsService)),
            ("Çekirdek", "ICommonServices", typeof(ICommonServices)),
            ("Çekirdek", "INavigationService", typeof(INavigationService)),
            ("Çekirdek", "IDialogService", typeof(IDialogService)),
            ("Log", "ILogService", typeof(ILogService)),
            ("Log", "IAppLogService", typeof(IAppLogService)),
            ("Log", "ISistemLogService", typeof(ISistemLogService)),
            ("Sistem Veritabanı", "ISistemDatabaseService", typeof(ISistemDatabaseService)),
            ("Sistem Veritabanı", "ISistemBackupManager", typeof(ISistemBackupManager)),
            ("Sistem Veritabanı", "ISistemMigrationManager", typeof(ISistemMigrationManager)),
            ("Tenant Veritabanı", "ITenantSQLiteDatabaseService", typeof(ITenantSQLiteDatabaseService)),
            ("Tenant Veritabanı", "ITenantSQLiteSelectionService", typeof(ITenantSQLiteSelectionService)),
            ("Tenant Veritabanı", "ITenantSQLiteDatabaseOperationService", typeof(ITenantSQLiteDatabaseOperationService)),
            ("Yedekleme", "ITenantSQLiteBackupManager", typeof(ITenantSQLiteBackupManager)),
            ("Yedekleme", "IDatabaseBackupManager", typeof(IDatabaseBackupManager)),
            ("Kimlik/Giriş", "IAuthenticationService", typeof(IAuthenticationService)),
            ("Kimlik/Giriş", "IIdentitySettingsProvider", typeof(IIdentitySettingsProvider)),
            ("Firma/Dönem", "IFirmaService", typeof(IFirmaService)),
            ("Firma/Dönem", "IMaliDonemService", typeof(IMaliDonemService)),
            ("Firma/Dönem", "IFirmaWithMaliDonemSelectedService", typeof(IFirmaWithMaliDonemSelectedService)),
            ("Lisans/Yetki", "ILisansService", typeof(ILisansService)),
            ("Lisans/Yetki", "IPermissionService", typeof(IPermissionService)),
            ("Lisans/Yetki", "IModuleLicenseService", typeof(IModuleLicenseService)),
            ("Lisans/Yetki", "IKullaniciService", typeof(IKullaniciService)),
            ("Güncelleme", "IUpdateService", typeof(IUpdateService)),
            ("Ayar Sağlayıcıları", "IAppPlatformSettingsProvider", typeof(IAppPlatformSettingsProvider)),
            ("Ayar Sağlayıcıları", "ITenantSettingsProvider", typeof(ITenantSettingsProvider)),
            ("Ayar Sağlayıcıları", "IDatabaseSettingsProvider", typeof(IDatabaseSettingsProvider)),
            ("Ayar Sağlayıcıları", "IEntityRegistrySettingsProvider", typeof(IEntityRegistrySettingsProvider)),
            ("Ayar Sağlayıcıları", "ILicenseSettingsProvider", typeof(ILicenseSettingsProvider)),
            ("Dev Araçları", "IDevModeProvider", typeof(IDevModeProvider)),
            ("Dev Araçları", "IDevAraclariService", typeof(IDevAraclariService)),
            ("Uygulama", "ISistemYasamDongusuService", typeof(ISistemYasamDongusuService)),
            ("Uygulama", "IGlobalAyarlarService", typeof(IGlobalAyarlarService)),
            ("Teşhis", "ISistemDiagnosticsService", typeof(ISistemDiagnosticsService)),
        };

        public async Task<IReadOnlyList<ModulTestSonucu>> CalistirAsync()
        {
            var sonuclar = new List<ModulTestSonucu>();

            // 1) DI kayıt/çözüm testleri (modül entegrasyonu — "uygulamaya bağlı mı?")
            foreach (var (modul, ad, servis) in KayitliServisler)
                sonuclar.Add(Cozumle(modul, ad, servis));

            // 2) Fonksiyonel smoke testleri (modül çalışıyor mu? — salt-okunur)
            sonuclar.Add(await CekirdekProbeAsync());
            sonuclar.Add(await SistemDbProbeAsync());
            sonuclar.Add(await TenantDbProbeAsync());
            sonuclar.Add(await GuncellemeProbeAsync());
            sonuclar.Add(await DevAracProbeAsync());

            return sonuclar;
        }

        private ModulTestSonucu Cozumle(string modul, string ad, Type tip)
        {
            try
            {
                var servis = _services.GetService(tip);
                return new ModulTestSonucu
                {
                    Modul = modul,
                    Ad = ad,
                    Basarili = servis != null,
                    Mesaj = servis != null ? "DI'da kayıtlı ve çözüldü." : "DI'da kayıtlı değil.",
                    Detay = tip.FullName ?? ad
                };
            }
            catch (Exception ex)
            {
                return new ModulTestSonucu
                {
                    Modul = modul,
                    Ad = ad,
                    Basarili = false,
                    Mesaj = "Çözümleme hatası: " + ex.Message,
                    Detay = ex.InnerException?.Message ?? tip.FullName ?? ad
                };
            }
        }

        private Task<ModulTestSonucu> CekirdekProbeAsync()
        {
            try
            {
                var paths = _services.GetService(typeof(IApplicationPaths)) as IApplicationPaths;
                if (paths == null)
                    return Task.FromResult(Hata("Çekirdek", "Yol/uygulama yolları", "IApplicationPaths çözülemedi."));

                var yol = paths.GetSistemDatabaseFilePath();
                var klasor = string.IsNullOrWhiteSpace(yol) ? null : Path.GetDirectoryName(yol);
                var klasorVar = !string.IsNullOrWhiteSpace(klasor) && Directory.Exists(klasor);
                return Task.FromResult(new ModulTestSonucu
                {
                    Modul = "Çekirdek",
                    Ad = "Yol/klasör erişimi",
                    Basarili = !string.IsNullOrWhiteSpace(yol) && klasorVar,
                    Mesaj = klasorVar ? "Uygulama veri klasörü erişilebilir." : "Veri klasörü bulunamadı.",
                    Detay = yol ?? "-"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(Hata("Çekirdek", "Yol/klasör erişimi", ex.Message));
            }
        }

        private async Task<ModulTestSonucu> SistemDbProbeAsync()
        {
            try
            {
                var svc = _services.GetService(typeof(ISistemDatabaseService)) as ISistemDatabaseService;
                if (svc == null)
                    return Hata("Sistem Veritabanı", "Durum okuma", "ISistemDatabaseService çözülemedi.");

                var cevap = await svc.GetSistemDatabaseStateAsync();
                var durum = cevap?.Data;
                return new ModulTestSonucu
                {
                    Modul = "Sistem Veritabanı",
                    Ad = "Durum okuma",
                    Basarili = durum != null && durum.CanConnect && !durum.HasError,
                    Mesaj = durum == null ? "Durum alınamadı." : $"Tablolar={durum.TableCount}, Geçerli={durum.DatabaseValid}, BekleyenGöç={durum.PendingMigrations?.Count ?? 0}",
                    Detay = durum?.CurrentVersion ?? "-"
                };
            }
            catch (Exception ex)
            {
                return Hata("Sistem Veritabanı", "Durum okuma", ex.Message);
            }
        }

        private Task<ModulTestSonucu> TenantDbProbeAsync()
        {
            try
            {
                var secim = _services.GetService(typeof(ITenantSQLiteSelectionService)) as ITenantSQLiteSelectionService;
                if (secim == null)
                    return Task.FromResult(Hata("Tenant Veritabanı", "Aktif dönem", "ITenantSQLiteSelectionService çözülemedi."));

                if (!secim.IsTenantLoaded || string.IsNullOrWhiteSpace(secim.CurrentTenant?.DatabaseName))
                {
                    return Task.FromResult(new ModulTestSonucu
                    {
                        Modul = "Tenant Veritabanı",
                        Ad = "Aktif dönem",
                        Basarili = true,
                        Mesaj = "Tenant yüklü değil (bilgi).",
                        Detay = "Bir dönem açıkken tekrar çalıştırın."
                    });
                }

                return Task.FromResult(new ModulTestSonucu
                {
                    Modul = "Tenant Veritabanı",
                    Ad = "Aktif dönem",
                    Basarili = true,
                    Mesaj = "Tenant bağlı.",
                    Detay = secim.CurrentTenant.DatabaseName
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(Hata("Tenant Veritabanı", "Aktif dönem", ex.Message));
            }
        }

        private async Task<ModulTestSonucu> GuncellemeProbeAsync()
        {
            try
            {
                var svc = _services.GetService(typeof(IUpdateService)) as IUpdateService;
                if (svc == null)
                    return Hata("Güncelleme", "Kaynak + normalize", "IUpdateService çözülemedi.");

                var ayar = await svc.GetSettingsAsync();
                var ozTest = AppGuncellemeBilgisi.OzTest();
                return new ModulTestSonucu
                {
                    Modul = "Güncelleme",
                    Ad = "Kaynak + normalize",
                    Basarili = ozTest.Basarili && !string.IsNullOrWhiteSpace(ayar?.FeedUrl),
                    Mesaj = ayar == null
                        ? "Ayar okunamadı."
                        : $"Kaynak tanımlı; normalize öz-testi {ozTest.Gecen}/{ozTest.Toplam}.",
                    Detay = $"FeedUrl: {ayar?.FeedUrl ?? "-"}"
                };
            }
            catch (Exception ex)
            {
                return Hata("Güncelleme", "Kaynak + normalize", ex.Message);
            }
        }

        private async Task<ModulTestSonucu> DevAracProbeAsync()
        {
            try
            {
                var svc = _services.GetService(typeof(IDevAraclariService)) as IDevAraclariService;
                if (svc == null)
                    return Hata("Dev Araçları", "Kimlik durumu", "IDevAraclariService çözülemedi.");

                var durum = await svc.DurumOkuAsync();
                return new ModulTestSonucu
                {
                    Modul = "Dev Araçları",
                    Ad = "Kimlik durumu",
                    Basarili = durum != null,
                    Mesaj = durum == null ? "Durum okunamadı." : "Kimlik/damga durumu okundu.",
                    Detay = durum == null ? "-" : $"Kurulum={durum.KurulumIdKisa}, Tenant kaydı={durum.TenantDamgalari?.Count ?? 0}"
                };
            }
            catch (Exception ex)
            {
                return Hata("Dev Araçları", "Kimlik durumu", ex.Message);
            }
        }

        private static ModulTestSonucu Hata(string modul, string ad, string mesaj) => new()
        {
            Modul = modul,
            Ad = ad,
            Basarili = false,
            Mesaj = mesaj
        };
    }
}
