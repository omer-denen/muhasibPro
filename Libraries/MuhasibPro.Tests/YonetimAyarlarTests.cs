using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;
using System.Diagnostics;

namespace MuhasibPro.Tests;

/// <summary>Firma-kapsamlı yönetim ayarları: anahtar şeması + izolasyon + fallback +
/// facade rozet/yetki/çakışma/yayın.</summary>
public class YonetimAyarlarTests
{
    private sealed class BellekAyarlari : ILocalSettingsService
    {
        public readonly Dictionary<string, object> Kutu = new();
        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (Kutu.TryGetValue(key, out var raw) && raw is T deger)
                return Task.FromResult<T?>(deger);
            return Task.FromResult<T?>(default);
        }
        public Task SaveSettingAsync<T>(string key, T value)
        {
            Kutu[key] = value!;
            return Task.CompletedTask;
        }
    }

    private sealed class KayitliBus : IEventBus
    {
        private readonly object _kilit = new();
        public readonly List<string> Yayinlar = new();
        public void Publish<TEvent>(object sender, TEvent @event) where TEvent : DomainEvent
        {
            if (@event is AppSettingsChangedEvent ayar)
                lock (_kilit) { Yayinlar.Add(ayar.SettingsKey); }
        }
        public void Subscribe<TEvent>(object target, Action<object, TEvent> handler) where TEvent : DomainEvent { }
        public void Unsubscribe(object target) { }
    }

    private static IAuthenticationService Kimlik(KullaniciRolTip rol)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.CurrentAccount).Returns(new HesapModel
        {
            KullaniciId = 1,
            KullaniciModel = new KullaniciModel
            {
                KullaniciAdi = "testadmin",
                Rol = new KullaniciRolModel { RolTip = rol }
            }
        });
        return auth.Object;
    }

    private static async Task<bool> BekleAsync(Func<bool> kosul, int zamanAsimiMs = 5000)
    {
        var kronometre = Stopwatch.StartNew();
        while (kronometre.ElapsedMilliseconds < zamanAsimiMs)
        {
            if (kosul()) return true;
            await Task.Delay(50);
        }
        return kosul();
    }

    private static YonetimAyarlarViewModel Facade(
        BellekAyarlari bellek, IAuthenticationService auth, KayitliBus bus, long firmaId)
    {
        var tenant = new TenantSettingsProvider(bellek, auth);
        var entity = new EntityRegistrySettingsProvider(bellek, auth);
        var vm = new YonetimAyarlarViewModel(Mock.Of<ICommonServices>(), bellek, tenant, entity, auth, bus)
        {
            FirmaId = firmaId
        };
        return vm;
    }

    [Fact]
    public void AnahtarSemasi_Kullanici_Sonekli_Global_Yalin()
    {
        KullaniciAyarAnahtari.KeyFor("TenantSettings", 5).Should().Be("TenantSettings:U5");
        KullaniciAyarAnahtari.KeyFor("TenantSettings", 0).Should().Be("TenantSettings");
        KullaniciAyarAnahtari.KeyFor("TenantSettings", -1).Should().Be("TenantSettings");
    }

    [Fact]
    public async Task FirmaKullaniciCozucu_KaydedenId_Doner()
    {
        var firma = new Mock<IFirmaService>();
        firma.Setup(s => s.GetByFirmaIdAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<FirmaModel>(new FirmaModel { Id = 7, KaydedenId = 5 }, "ok"));
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IFirmaService))).Returns(firma.Object);
        var coz = new FirmaKullaniciCozucu(sp.Object, Kimlik(KullaniciRolTip.Yönetici));

        (await coz.CozAsync(7)).Should().Be(5);
        (await coz.CozAsync(0)).Should().Be(0);
        coz.GirisYapanId().Should().Be(1);
    }

    [Fact]
    public async Task FirmaKullaniciCozucu_Cozulemezse_Sifir()
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IFirmaService))).Returns((object)null!);
        var coz = new FirmaKullaniciCozucu(sp.Object);

        (await coz.CozAsync(7)).Should().Be(0);
        coz.GirisYapanId().Should().Be(0);
    }

    [Fact]
    public void AnahtarSemasi_Firma_Sonekli_Global_Yalin()
    {
        FirmaAyarAnahtari.KeyFor("TenantSettings", 7).Should().Be("TenantSettings:F7");
        FirmaAyarAnahtari.KeyFor("TenantSettings", 0).Should().Be("TenantSettings");
        FirmaAyarAnahtari.KeyFor("TenantSettings", -1).Should().Be("TenantSettings");
    }

    private static IFirmaKullaniciCozucu Cozucu(long giris, Func<long, long> firmaCoz)
    {
        var coz = new Mock<IFirmaKullaniciCozucu>();
        coz.Setup(c => c.GirisYapanId()).Returns(giris);
        coz.Setup(c => c.CozAsync(It.IsAny<long>())).ReturnsAsync((long f) => firmaCoz(f));
        return coz.Object;
    }

    [Fact]
    public async Task Cozucusuz_Save_Global_Yazar()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new TenantSettings { YedekPageSize = 10 }, firmaId: 7);

        (await saglayici.GetAsync(7)).GetYedekPageSize().Should().Be(10);
        (await saglayici.GetAsync()).GetYedekPageSize().Should().Be(10);
    }

    [Fact]
    public async Task KullaniciIzolasyonu_Firma_Kaydedenine_Ait()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new TenantSettingsProvider(
            bellek, Kimlik(KullaniciRolTip.Yönetici), Cozucu(giris: 5, f => f == 7 ? 5 : 0));

        await saglayici.SaveAsync(new TenantSettings { YedekPageSize = 10 }, firmaId: 7);

        (await saglayici.GetAsync(7)).GetYedekPageSize().Should().Be(10);
        (await saglayici.GetAsync(9)).GetYedekPageSize().Should().Be(4);
        bellek.Kutu.Should().ContainKey("TenantSettings:U5");
    }

    [Fact]
    public async Task EskiFirmaKaydi_Kullaniciya_Tasinir()
    {
        var bellek = new BellekAyarlari();
        await bellek.SaveSettingAsync("TenantSettings:F7", new TenantSettings { YedekPageSize = 10 });
        var saglayici = new TenantSettingsProvider(
            bellek, Kimlik(KullaniciRolTip.Yönetici), Cozucu(giris: 5, f => f == 7 ? 5 : 0));

        (await saglayici.GetAsync(7)).GetYedekPageSize().Should().Be(10);

        bellek.Kutu.Should().ContainKey("TenantSettings:U5");
    }

    [Fact]
    public async Task Anahtarsiz_Firma_Global_Sablona_Duser()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));

        await saglayici.SaveAsync(new TenantSettings { YedekPageSize = 9 });

        (await saglayici.GetAsync(7)).GetYedekPageSize().Should().Be(9);
    }

    [Fact]
    public async Task Entity_KullaniciIzolasyonu()
    {
        var bellek = new BellekAyarlari();
        var saglayici = new EntityRegistrySettingsProvider(
            bellek, Kimlik(KullaniciRolTip.Yönetici), Cozucu(giris: 5, f => f == 7 ? 5 : 0));

        await saglayici.SaveAsync(new EntityRegistrySettings { AcikPageSize = 12 }, firmaId: 7);

        (await saglayici.GetAsync(7)).GetAcikPageSize().Should().Be(12);
        (await saglayici.GetAsync(9)).GetAcikPageSize().Should().Be(8);
        bellek.Kutu.Should().ContainKey("EntityRegistrySettings:U5");
    }

    [Fact]
    public async Task Rozet_Yokken_Varsayilan_Degisince_Ozellestirilmis()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Yönetici), bus, firmaId: 7);
        await vm.LoadAsync();

        vm.IsOzel.Should().BeFalse();
        vm.RozetMetni.Should().Be("Varsayılan");

        vm.Liste.YedekPageSize = 9;
        (await BekleAsync(() => vm.IsOzel)).Should().BeTrue();
        vm.RozetMetni.Should().Be("Özelleştirilmiş");
        (await BekleAsync(() => bus.Yayinlar.Contains("TenantSettings:F7"))).Should().BeTrue();

        await vm.SifirlaAsync();
        vm.IsOzel.Should().BeFalse();
        vm.Liste.YedekPageSize.Should().Be(4);
    }

    [Fact]
    public async Task NormalKullanici_Saklama_Limitini_Degistiremez()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Kullanici), bus, firmaId: 7);
        await vm.LoadAsync();

        vm.Saklama.MaxManuelYedekSayisi = 9;
        (await BekleAsync(() => vm.HasHata)).Should().BeTrue();
        vm.Saklama.MaxManuelYedekSayisi.Should().Be(5);
    }

    [Fact]
    public async Task Cakisma_Bar_Gosterir_Onaysiz_Yazmaz_Onayla_Yazar()
    {
        var bellek = new BellekAyarlari();
        var yonetici = Kimlik(KullaniciRolTip.Yönetici);
        var bus = new KayitliBus();
        var vm = Facade(bellek, yonetici, bus, firmaId: 7);
        await vm.LoadAsync();

        // Başka yönetici (B) doğrudan provider'dan yazar.
        var diger = new TenantSettingsProvider(bellek, yonetici);
        await diger.SaveAsync(new TenantSettings { YedekPageSize = 12 }, firmaId: 7);

        // A yazar → çakışma barı, yazım durur.
        vm.Liste.YedekPageSize = 9;
        (await BekleAsync(() => vm.ConflictVar)).Should().BeTrue();
        (await diger.GetAsync(7)).GetYedekPageSize().Should().Be(12);

        // Vazgeç → B'nin değeri ekrana gelir.
        await vm.OnConflictVazgecAsync();
        vm.Liste.YedekPageSize.Should().Be(12);
        vm.ConflictVar.Should().BeFalse();

        // B tekrar yazar, A yine yazar → bar, bu kez üzerine yaz.
        await diger.SaveAsync(new TenantSettings { YedekPageSize = 14 }, firmaId: 7);
        vm.Liste.YedekPageSize = 9;
        (await BekleAsync(() => vm.ConflictVar)).Should().BeTrue();
        await vm.OnConflictOverwriteAsync();
        (await BekleAsync(() => !vm.ConflictVar)).Should().BeTrue();
        (await diger.GetAsync(7)).GetYedekPageSize().Should().Be(9);
    }

    [Fact]
    public async Task DurumButonu_Varsayilanda_Pasif_Ozelde_Aktif()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Yönetici), bus, firmaId: 7);
        await vm.LoadAsync();

        vm.DurumButonMetni.Should().Be("Varsayılan");
        vm.DurumButonIpucu.Should().Contain("şablon değerleri kullanıyor");

        vm.Liste.YedekPageSize = 9;
        (await BekleAsync(() => vm.IsOzel)).Should().BeTrue();
        vm.DurumButonMetni.Should().Be("Varsayılanlara Dön");
        vm.DurumButonIpucu.Should().Contain("firma anahtarı sıfırlanır");
    }

    [Fact]
    public async Task Kaydedildi_Bildirimi_Basarili_Kayitta_Gosterilir()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Yönetici), bus, firmaId: 7);
        await vm.LoadAsync();

        vm.HasKaydedildi.Should().BeFalse();
        vm.Liste.YedekPageSize = 9;
        (await BekleAsync(() => vm.HasKaydedildi)).Should().BeTrue();
        vm.KaydedildiMetni.Should().StartWith("Kaydedildi •");
    }

    private static YonetimAyarlarViewModel GenelFacade(
        BellekAyarlari bellek, IFirmaService firma, IKullaniciService kullanici, long firmaId)
    {
        var tenant = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));
        var entity = new EntityRegistrySettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));
        return new YonetimAyarlarViewModel(Mock.Of<ICommonServices>(), bellek, tenant, entity,
            Kimlik(KullaniciRolTip.Yönetici), new KayitliBus(), firma, kullanici)
        {
            FirmaId = firmaId
        };
    }

    [Fact]
    public async Task Genel_Firma_Servisle_Dolar_Olusturan_Bulunamazsa_Id_Duser()
    {
        var bellek = new BellekAyarlari();
        var firma = new Mock<IFirmaService>();
        firma.Setup(f => f.GetByFirmaIdAsync(7)).ReturnsAsync(new ApiDataResponse<FirmaModel>(
            new FirmaModel
            {
                Id = 7,
                FirmaKodu = "F-0007",
                KisaUnvani = "Test Firma",
                YetkiliKisi = "Ali Veli",
                Il = "Ankara",
                VergiDairesi = "Ulus",
                VergiNo = "123",
                Telefon1 = "03120000000",
                Eposta = "a@b.c",
                KayitTarihi = new DateTime(2026, 1, 2, 3, 4, 5),
                KaydedenId = 99
            }, string.Empty, true, ResultCodes.BASARILI_Tamamlandi, 1));
        var kullanici = new Mock<IKullaniciService>();
        kullanici.Setup(k => k.GetKullaniciAsync(99)).ReturnsAsync(
            new ApiDataResponse<KullaniciModel>(null!, "yok", false, ResultCodes.HATA_Bulunamadi, 0));

        var vm = GenelFacade(bellek, firma.Object, kullanici.Object, firmaId: 7);
        await vm.LoadAsync();

        vm.Genel.Firma.Should().NotBeNull();
        vm.Genel.Firma.Initials.Should().Be("TF");
        vm.Genel.FirmaDetaySatiri.Should().Contain("Ali Veli").And.Contain("Ulus 123");
        vm.Genel.OlusturanAdi.Should().Be("ID: 99");
        vm.Genel.KayitTarihiMetni.Should().StartWith("Kayıt: ");
    }

    [Fact]
    public async Task PresetDisi_Kayit_EnYakina_Cekilir_Sahte_Cakisma_Yok()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var tenant = new TenantSettingsProvider(bellek, Kimlik(KullaniciRolTip.Yönetici));
        await tenant.SaveAsync(new TenantSettings { YedekPageSize = 9 }, firmaId: 7);
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Yönetici), bus, firmaId: 7);
        await vm.LoadAsync();

        // Ham 9 preset-dışı: ekranda en yakın (8), rozette özel.
        vm.Liste.YedekPageSize.Should().Be(8);
        vm.IsOzel.Should().BeTrue();

        // Başka alanın kaydı sahte çakışma üretmez; ilk kayıt ham değeri de yakınsar.
        vm.Liste.BilinmeyenPageSize = 8;
        (await BekleAsync(() => vm.HasKaydedildi)).Should().BeTrue();
        vm.ConflictVar.Should().BeFalse();
        (await tenant.GetAsync(7)).GetYedekPageSize().Should().Be(8);
    }

    [Fact]
    public async Task Genel_Servis_Yoksa_Bos_Kalir()
    {
        var bellek = new BellekAyarlari();
        var bus = new KayitliBus();
        var vm = Facade(bellek, Kimlik(KullaniciRolTip.Yönetici), bus, firmaId: 7);
        await vm.LoadAsync();

        vm.Genel.Firma.Should().BeNull();
        vm.Genel.FirmaDetaySatiri.Should().BeEmpty();
        vm.Genel.OlusturanAdi.Should().BeEmpty();
        vm.Genel.KayitTarihiMetni.Should().BeEmpty();
    }
}
