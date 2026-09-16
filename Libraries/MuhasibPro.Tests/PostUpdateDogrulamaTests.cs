using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.UpdateDogrulama;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Helpers;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models.PostUpdateModel;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Sistem;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.91-D: güncelleme sonrası doğrulama — orkestratör + dosya + Sistem.db + dönem matrisi.</summary>
public class PostUpdateDogrulamaTests
{
    // ---------------- ortak yardımcılar ----------------

    private static ApiDataResponse<DatabaseConnectionAnalysis> DbState(
        bool exists = true, bool connect = true, bool hasError = false, bool valid = true,
        List<string>? pending = null, bool future = false, string version = "1.1.0")
        => new SuccessApiDataResponse<DatabaseConnectionAnalysis>(
            new DatabaseConnectionAnalysis
            {
                IsDatabaseExists = exists,
                CanConnect = connect,
                HasError = hasError,
                DatabaseValid = valid,
                PendingMigrations = pending ?? new List<string>(),
                IsFutureSchema = future,
                CurrentVersion = version
            }, "durum");

    private static Mock<ILocalSettingsService> AyarServisi(UpdateSettingsModel? ayar)
    {
        UpdateSettingsModel? current = ayar;
        var mock = new Mock<ILocalSettingsService>();
        mock.Setup(l => l.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey))
            .ReturnsAsync(() => current);
        mock.Setup(l => l.SaveSettingAsync(UpdateSettingsModel.SettingsKey, It.IsAny<UpdateSettingsModel>()))
            .Callback<string, UpdateSettingsModel>((_, m) => current = m)
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static UpdateSettingsModel Ayar(string? to = "1.1.4", string? yedek = "C:\\bk\\pre.backup",
        DateTime? verified = null, bool pending = true)
        => new()
        {
            LastUpdateFromVersion = "1.1.3",
            LastUpdateToVersion = to,
            LastUpdateBackupPath = yedek,
            LastUpdateStartTime = DateTime.Now,
            LastUpdateVerifiedAt = verified,
            PostUpdatePending = pending
        };

    private static string GeciciDosya(string icerik = "x")
    {
        var yol = Path.Combine(Path.GetTempPath(), "muhasib_" + Guid.NewGuid().ToString("N") + ".tmp");
        File.WriteAllText(yol, icerik);
        return yol;
    }

    // ---------------- orkestratör: tetik ----------------

    [Fact]
    public async Task GerekliMi_PendingTrue_True()
    {
        var svc = new PostUpdateDogrulamaService(
            Mock.Of<IUygulamaDosyaDogrulayici>(), Mock.Of<ISistemDbGocDogrulayici>(),
            Mock.Of<ITenantTaramaDogrulayici>(), AyarServisi(Ayar(pending: true)).Object, Mock.Of<ISistemLogService>());

        (await svc.GerekliMiAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task GerekliMi_PendingFalse_False()
    {
        var svc = new PostUpdateDogrulamaService(
            Mock.Of<IUygulamaDosyaDogrulayici>(), Mock.Of<ISistemDbGocDogrulayici>(),
            Mock.Of<ITenantTaramaDogrulayici>(), AyarServisi(Ayar(pending: false)).Object, Mock.Of<ISistemLogService>());

        (await svc.GerekliMiAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task GerekliMi_AyarYoksa_False()
    {
        var svc = new PostUpdateDogrulamaService(
            Mock.Of<IUygulamaDosyaDogrulayici>(), Mock.Of<ISistemDbGocDogrulayici>(),
            Mock.Of<ITenantTaramaDogrulayici>(), AyarServisi(null).Object, Mock.Of<ISistemLogService>());

        (await svc.GerekliMiAsync()).Should().BeFalse();
    }

    // ---------------- orkestratör: akış ----------------

    private static PostUpdateDogrulamaService Orkestrator(
        IUygulamaDosyaDogrulayici uygulama,
        ISistemDbGocDogrulayici sistem,
        ITenantTaramaDogrulayici tenant,
        Mock<ILocalSettingsService> local)
        => new(uygulama, sistem, tenant, local.Object, Mock.Of<ISistemLogService>());

    [Fact]
    public async Task Calistir_Basarili_AdimlarVeDamga()
    {
        var local = AyarServisi(Ayar());
        var uygulama = new Mock<IUygulamaDosyaDogrulayici>();
        uygulama.Setup(u => u.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Ok("dosyalar ok"));
        var sistem = new Mock<ISistemDbGocDogrulayici>();
        sistem.Setup(s => s.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Ok("sistem ok"));
        var tenant = new Mock<ITenantTaramaDogrulayici>();
        tenant.Setup(t => t.TaraAsync(It.IsAny<IProgress<double>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantTaramaSonucu { Taranan = 2, Mesaj = "2 dönem tarandı" });

        var rapor = new List<PostUpdateAdimSonucu>();
        var sonuc = await Orkestrator(uygulama.Object, sistem.Object, tenant.Object, local)
            .CalistirAsync(new Progress<PostUpdateAdimSonucu>(rapor.Add));

        sonuc.Basarili.Should().BeTrue();
        sonuc.Bloklayici.Should().BeFalse();
        sonuc.SonucTuru.Should().Be(PostUpdateSonucTuru.Temiz);
        sonuc.Adimlar.Should().HaveCount(3);
        sonuc.Adimlar.Should().OnlyContain(a => a.Durum == PostUpdateAdimDurumu.Basarili);
        var kayit = await local.Object.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
        kayit!.LastUpdateVerifiedAt.Should().NotBeNull();
        kayit.PostUpdatePending.Should().BeFalse();
        rapor.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Calistir_UygulamaBloklayici_DigerAdimlarAtlanir_DamgaVar()
    {
        var local = AyarServisi(Ayar(pending: true));
        var uygulama = new Mock<IUygulamaDosyaDogrulayici>();
        uygulama.Setup(u => u.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Block("dosya eksik"));
        var sistem = new Mock<ISistemDbGocDogrulayici>();
        var tenant = new Mock<ITenantTaramaDogrulayici>();

        var sonuc = await Orkestrator(uygulama.Object, sistem.Object, tenant.Object, local).CalistirAsync();

        sonuc.Basarili.Should().BeFalse();
        sonuc.Bloklayici.Should().BeFalse();
        sonuc.SonucTuru.Should().Be(PostUpdateSonucTuru.UygulamaBasarisiz);
        sonuc.Baslik.Should().Be("Uygulama güncellenemedi");
        sonuc.Ozet.Should().Contain("dosya eksik");
        sonuc.Adimlar.Should().HaveCount(3);
        sonuc.Adimlar.Count(a => a.Durum == PostUpdateAdimDurumu.Atlandi).Should().Be(2);
        sistem.Verify(s => s.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        tenant.Verify(t => t.TaraAsync(It.IsAny<IProgress<double>?>(), It.IsAny<CancellationToken>()), Times.Never);
        // Uygulama hatası tek sefer gösterilir: damga atılır, bekleyen bayrak temizlenir.
        var kayit = await local.Object.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
        kayit!.LastUpdateVerifiedAt.Should().NotBeNull();
        kayit.PostUpdatePending.Should().BeFalse();
    }

    [Fact]
    public async Task Calistir_SistemBloklayici_DonemAtlanir_DamgaYok()
    {
        var local = AyarServisi(Ayar(pending: true));
        var uygulama = new Mock<IUygulamaDosyaDogrulayici>();
        uygulama.Setup(u => u.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Ok("ok"));
        var sistem = new Mock<ISistemDbGocDogrulayici>();
        sistem.Setup(s => s.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Block("geri alınamadı"));
        var tenant = new Mock<ITenantTaramaDogrulayici>();

        var sonuc = await Orkestrator(uygulama.Object, sistem.Object, tenant.Object, local).CalistirAsync();

        sonuc.Basarili.Should().BeFalse();
        sonuc.Bloklayici.Should().BeTrue();
        sonuc.SonucTuru.Should().Be(PostUpdateSonucTuru.SistemBasarisiz);
        sonuc.Baslik.Should().Be("Sistem veritabanı güncellenemedi");
        sonuc.Adimlar.Should().Contain(a => a.Ad == "Mali Dönem Veritabanları" && a.Durum == PostUpdateAdimDurumu.Atlandi);
        tenant.Verify(t => t.TaraAsync(It.IsAny<IProgress<double>?>(), It.IsAny<CancellationToken>()), Times.Never);
        // Sert blok: damga atılmaz, bayrak temizlenmez → sonraki açılışta tekrar denenir.
        var kayit = await local.Object.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
        kayit!.LastUpdateVerifiedAt.Should().BeNull();
        kayit.PostUpdatePending.Should().BeTrue();
    }

    [Fact]
    public async Task Calistir_DonemBozuk_SonucUyariylaBasarili_DamgaVar()
    {
        var local = AyarServisi(Ayar());
        var uygulama = new Mock<IUygulamaDosyaDogrulayici>();
        uygulama.Setup(u => u.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Ok("ok"));
        var sistem = new Mock<ISistemDbGocDogrulayici>();
        sistem.Setup(s => s.DogrulaAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DogrulamaAdimSonucu.Ok("ok"));
        var tenant = new Mock<ITenantTaramaDogrulayici>();
        tenant.Setup(t => t.TaraAsync(It.IsAny<IProgress<double>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantTaramaSonucu
            {
                Taranan = 1,
                Bozuk = 1,
                Mesaj = "1 dönem tarandı • 1 bozuk",
                RaporSatirlari = new List<string> { "db-1 • bozuk, kurtarılamadı" }
            });

        var sonuc = await Orkestrator(uygulama.Object, sistem.Object, tenant.Object, local).CalistirAsync();

        sonuc.Basarili.Should().BeTrue();
        sonuc.Bloklayici.Should().BeFalse();
        sonuc.SonucTuru.Should().Be(PostUpdateSonucTuru.Dikkat);
        sonuc.BozukDonemSayisi.Should().Be(1);
        sonuc.RaporSatirlari.Should().ContainSingle();
        sonuc.Adimlar.Should().Contain(a => a.Durum == PostUpdateAdimDurumu.Uyari);
        var kayit = await local.Object.ReadSettingAsync<UpdateSettingsModel>(UpdateSettingsModel.SettingsKey);
        kayit!.LastUpdateVerifiedAt.Should().NotBeNull();
        kayit.PostUpdatePending.Should().BeFalse();
    }

    // ---------------- uygulama dosyası doğrulayıcı ----------------

    [Fact]
    public async Task UygulamaDosya_HepsiVarsa_Ok()
    {
        var dir = Path.Combine(Path.GetTempPath(), "muhasib_dir_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.dll"), "x");
            File.WriteAllText(Path.Combine(dir, "b.dll"), "x");
            var svc = new UygulamaDosyaDogrulayici(dir, new[] { "a.dll", "b.dll" });

            var sonuc = await svc.DogrulaAsync(ProcessInfoHelper.Version);

            sonuc.Basarili.Should().BeTrue();
            sonuc.Uyari.Should().BeFalse();
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task UygulamaDosya_Eksikse_Bloklar()
    {
        var dir = Path.Combine(Path.GetTempPath(), "muhasib_dir_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.dll"), "x");
            var svc = new UygulamaDosyaDogrulayici(dir, new[] { "a.dll", "b.dll" });

            var sonuc = await svc.DogrulaAsync(null);

            sonuc.Bloklayici.Should().BeTrue();
            sonuc.Mesaj.Should().Contain("b.dll");
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task UygulamaDosya_SurumFarkliysa_Hata()
    {
        var dir = Path.Combine(Path.GetTempPath(), "muhasib_dir_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.dll"), "x");
            var svc = new UygulamaDosyaDogrulayici(dir, new[] { "a.dll" });

            var sonuc = await svc.DogrulaAsync("99.99.99");

            sonuc.Basarili.Should().BeFalse();
            sonuc.Uyari.Should().BeFalse();
            sonuc.Bloklayici.Should().BeFalse();
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task UygulamaDosya_OnSurumEki_EsitSayilir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "muhasib_dir_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.dll"), "x");
            var svc = new UygulamaDosyaDogrulayici(dir, new[] { "a.dll" });
            // Çalışan sürüm 1.1.x iken hedef "1.1.4-beta" olsa bile ön-sürüm eki yok sayılır:
            // karşılaştırma yalnız major.minor.patch üzerinden yapılır (uyuşmazsa hata, eşitse ok).
            var hedef = ProcessInfoHelper.Version + "-beta";

            var sonuc = await svc.DogrulaAsync(hedef);

            sonuc.Basarili.Should().BeTrue();
            sonuc.Uyari.Should().BeFalse();
        }
        finally { Directory.Delete(dir, true); }
    }

    // ---------------- Sistem.db göç doğrulayıcı ----------------

    private static SistemDbGocDogrulayici SistemServis(
        Mock<ISistemDatabaseService> sistem, Mock<ISistemDatabaseOperationService> operasyon)
        => new(sistem.Object, operasyon.Object);

    [Fact]
    public async Task Sistem_FutureSchema_Bloklar_Yazmaz()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(future: true, version: "9.9.0"));
        var operasyon = new Mock<ISistemDatabaseOperationService>();

        var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync("C:\\yok.backup");

        sonuc.Bloklayici.Should().BeTrue();
        sistem.Verify(s => s.InitializeSistemDatabaseAsync(), Times.Never);
    }

    [Fact]
    public async Task Sistem_Saglikliyse_Ok()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync()).ReturnsAsync(DbState());
        var operasyon = new Mock<ISistemDatabaseOperationService>();

        var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync(null);

        sonuc.Basarili.Should().BeTrue();
        sistem.Verify(s => s.InitializeSistemDatabaseAsync(), Times.Never);
    }

    [Fact]
    public async Task Sistem_BekleyenGoc_GocVeVerify()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.SetupSequence(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(pending: new List<string> { "M1" }))   // ilk
            .ReturnsAsync(DbState());                                    // göç sonrası
        sistem.Setup(s => s.InitializeSistemDatabaseAsync()).ReturnsAsync((true, "göç ok"));
        var operasyon = new Mock<ISistemDatabaseOperationService>();

        var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync(null);

        sonuc.Basarili.Should().BeTrue();
        sonuc.Uyari.Should().BeFalse();
        sistem.Verify(s => s.InitializeSistemDatabaseAsync(), Times.Once);
    }

    [Fact]
    public async Task Sistem_GocDuser_YedekYoksa_Bloklar()
    {
        var sistem = new Mock<ISistemDatabaseService>();
        sistem.Setup(s => s.GetSistemDatabaseStateAsync())
            .ReturnsAsync(DbState(valid: false, connect: false, hasError: true));
        sistem.Setup(s => s.InitializeSistemDatabaseAsync()).ReturnsAsync((false, "göç hatası"));
        var operasyon = new Mock<ISistemDatabaseOperationService>();

        var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync(null);

        sonuc.Bloklayici.Should().BeTrue();
        operasyon.Verify(o => o.RestoreBackupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Sistem_GocDuser_YedektenRestore_Basarili()
    {
        var yedekYolu = GeciciDosya();
        try
        {
            var sistem = new Mock<ISistemDatabaseService>();
            sistem.SetupSequence(s => s.GetSistemDatabaseStateAsync())
                .ReturnsAsync(DbState(valid: false, connect: false, hasError: true)) // ilk
                .ReturnsAsync(DbState(valid: false, connect: false, hasError: true)) // göç sonrası (başarısız)
                .ReturnsAsync(DbState());                                            // restore sonrası
            sistem.Setup(s => s.InitializeSistemDatabaseAsync()).ReturnsAsync((false, "göç hatası"));
            var operasyon = new Mock<ISistemDatabaseOperationService>();
            operasyon.Setup(o => o.RestoreBackupAsync(yedekYolu)).ReturnsAsync(
                new SuccessApiDataResponse<DatabaseRestoreExecutionResult>(
                    new DatabaseRestoreExecutionResult { IsRestoreSuccess = true }, "restore ok"));

            var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync(yedekYolu);

            sonuc.Basarili.Should().BeTrue();
            sonuc.Uyari.Should().BeTrue();
            operasyon.Verify(o => o.RestoreBackupAsync(yedekYolu), Times.Once);
        }
        finally { File.Delete(yedekYolu); }
    }

    [Fact]
    public async Task Sistem_GocVeRestoreDuser_Bloklar()
    {
        var yedekYolu = GeciciDosya();
        try
        {
            var sistem = new Mock<ISistemDatabaseService>();
            sistem.Setup(s => s.GetSistemDatabaseStateAsync())
                .ReturnsAsync(DbState(valid: false, connect: false, hasError: true));
            sistem.Setup(s => s.InitializeSistemDatabaseAsync()).ReturnsAsync((false, "göç hatası"));
            var operasyon = new Mock<ISistemDatabaseOperationService>();
            operasyon.Setup(o => o.RestoreBackupAsync(yedekYolu)).ReturnsAsync(
                new ErrorApiDataResponse<DatabaseRestoreExecutionResult>(null!, "restore hatası"));

            var sonuc = await SistemServis(sistem, operasyon).DogrulaAsync(yedekYolu);

            sonuc.Bloklayici.Should().BeTrue();
        }
        finally { File.Delete(yedekYolu); }
    }

    // ---------------- dönem tarama ----------------

    private static TenantTaramaDogrulayici Tarama(
        Mock<IMaliDonemService> donem,
        Mock<ITenantSQLiteDatabaseLifecycleService> lifecycle,
        Mock<ITenantSQLiteDatabaseOperationService> operasyon,
        Mock<IApplicationPaths> yollar)
        => new(donem.Object, lifecycle.Object, operasyon.Object, yollar.Object, Mock.Of<ISistemLogService>());

    private static Mock<IApplicationPaths> Yollar(params string[] mevcut)
    {
        var mock = new Mock<IApplicationPaths>();
        mock.Setup(p => p.TenantDatabaseFileExists(It.IsAny<string>()))
            .Returns<string>(ad => mevcut.Contains(ad));
        return mock;
    }

    private static Mock<IMaliDonemService> Donemler(params string[] adlar)
    {
        var liste = adlar.Select(ad => new MaliDonemModel { DatabaseName = ad }).ToList();
        var mock = new Mock<IMaliDonemService>();
        mock.Setup(m => m.GetMaliDonemlerPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync(new SuccessApiDataResponse<IList<MaliDonemModel>>(liste, "ok"));
        return mock;
    }

    [Fact]
    public async Task Tarama_DonemYoksa_MesajDoner()
    {
        var sonuc = await Tarama(Donemler(), new Mock<ITenantSQLiteDatabaseLifecycleService>(),
            new Mock<ITenantSQLiteDatabaseOperationService>(), Yollar()).TaraAsync();

        sonuc.Taranan.Should().Be(0);
        sonuc.Mesaj.Should().Contain("yok");
    }

    [Fact]
    public async Task Tarama_Saglikli_RaporSatiriYok()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();
        lifecycle.Setup(l => l.GetTenantDatabaseStateAsync("db-1")).ReturnsAsync(DbState());

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, new Mock<ITenantSQLiteDatabaseOperationService>(), Yollar("db-1")).TaraAsync();

        sonuc.Taranan.Should().Be(1);
        sonuc.RaporSatirlari.Should().BeEmpty();
        sonuc.Bekleyen.Should().Be(0);
    }

    [Fact]
    public async Task Tarama_FutureSchema_Raporlanir_Dokunmaz()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();
        lifecycle.Setup(l => l.GetTenantDatabaseStateAsync("db-1"))
            .ReturnsAsync(DbState(future: true, version: "9.9.0"));
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, operasyon, Yollar("db-1")).TaraAsync();

        sonuc.GelecekSema.Should().Be(1);
        sonuc.RaporSatirlari.Should().ContainSingle();
        operasyon.Verify(o => o.RestoreFromLatestBackupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Tarama_Bozuk_YedektenKurtarilir()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();
        lifecycle.SetupSequence(l => l.GetTenantDatabaseStateAsync("db-1"))
            .ReturnsAsync(DbState(valid: false, connect: false, hasError: true)) // ilk
            .ReturnsAsync(DbState());                                            // restore sonrası
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.RestoreFromLatestBackupAsync("db-1"))
            .ReturnsAsync(new SuccessApiDataResponse<bool>(true, "restore ok"));

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, operasyon, Yollar("db-1")).TaraAsync();

        sonuc.Kurtarilan.Should().Be(1);
        sonuc.Bozuk.Should().Be(0);
    }

    [Fact]
    public async Task Tarama_Bozuk_YedekYoksa_Raporlanir()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();
        lifecycle.Setup(l => l.GetTenantDatabaseStateAsync("db-1"))
            .ReturnsAsync(DbState(valid: false, connect: false, hasError: true));
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();
        operasyon.Setup(o => o.RestoreFromLatestBackupAsync("db-1"))
            .ReturnsAsync(new SuccessApiDataResponse<bool>(false, "yedek yok"));

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, operasyon, Yollar("db-1")).TaraAsync();

        sonuc.Bozuk.Should().Be(1);
        sonuc.Kurtarilan.Should().Be(0);
    }

    [Fact]
    public async Task Tarama_Pending_YalnizRapor()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();
        lifecycle.Setup(l => l.GetTenantDatabaseStateAsync("db-1"))
            .ReturnsAsync(DbState(pending: new List<string> { "M1", "M2" }));
        var operasyon = new Mock<ITenantSQLiteDatabaseOperationService>();

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, operasyon, Yollar("db-1")).TaraAsync();

        sonuc.Bekleyen.Should().Be(1);
        sonuc.RaporSatirlari.Should().ContainSingle();
        operasyon.Verify(o => o.RestoreFromLatestBackupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Tarama_DosyasiOlmayanDonem_Atlanir()
    {
        var lifecycle = new Mock<ITenantSQLiteDatabaseLifecycleService>();

        var sonuc = await Tarama(Donemler("db-1"), lifecycle, new Mock<ITenantSQLiteDatabaseOperationService>(), Yollar()).TaraAsync();

        sonuc.Taranan.Should().Be(0);
        lifecycle.Verify(l => l.GetTenantDatabaseStateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Tarama_ListePatlarsa_Taranamadi()
    {
        var donem = new Mock<IMaliDonemService>();
        donem.Setup(m => m.GetMaliDonemlerPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DataRequest<MaliDonem>>()))
            .ThrowsAsync(new InvalidOperationException("db yok"));

        var sonuc = await Tarama(donem, new Mock<ITenantSQLiteDatabaseLifecycleService>(),
            new Mock<ITenantSQLiteDatabaseOperationService>(), Yollar()).TaraAsync();

        sonuc.Taranamadi.Should().BeTrue();
        sonuc.Mesaj.Should().Contain("okunamadı");
    }

    // ---------------- ViewModel: uyarı/devam mantığı ----------------

    private static GuncellemeSonrasiViewModel ViewModel(Mock<IPostUpdateDogrulamaService> saga)
        => new(saga.Object, Mock.Of<ICommonServices>());

    private static PostUpdateDogrulamaSonucu Sonuc(PostUpdateSonucTuru tur, string baslik, params PostUpdateAdimDurumu[] durumlar)
    {
        var s = new PostUpdateDogrulamaSonucu
        {
            SonucTuru = tur,
            Baslik = baslik,
            Ozet = "özet",
            Basarili = tur is PostUpdateSonucTuru.Temiz or PostUpdateSonucTuru.Dikkat,
            Bloklayici = tur == PostUpdateSonucTuru.SistemBasarisiz
        };
        foreach (var d in durumlar)
            s.Adimlar.Add(new PostUpdateAdimSonucu { Ad = "X", Durum = d });
        return s;
    }

    [Fact]
    public async Task ViewModel_Temiz_OtomatikGecis()
    {
        var saga = new Mock<IPostUpdateDogrulamaService>();
        saga.Setup(s => s.CalistirAsync(It.IsAny<IProgress<PostUpdateAdimSonucu>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sonuc(PostUpdateSonucTuru.Temiz, "Güncelleme tamamlandı", PostUpdateAdimDurumu.Basarili));

        var vm = ViewModel(saga);
        await vm.CalistirAsync();

        vm.SonucHazir.Should().BeTrue();
        vm.Bloklayici.Should().BeFalse();
        vm.UyariVar.Should().BeFalse();
        vm.DevamEtGorunur.Should().BeTrue();
        vm.OtomatikGecis.Should().BeTrue();
        vm.SonucBaslik.Should().Be("Güncelleme tamamlandı");
    }

    [Fact]
    public async Task ViewModel_Dikkat_DevamEtAmaOtomatikGecmez()
    {
        var saga = new Mock<IPostUpdateDogrulamaService>();
        saga.Setup(s => s.CalistirAsync(It.IsAny<IProgress<PostUpdateAdimSonucu>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sonuc(PostUpdateSonucTuru.Dikkat, "Güncelleme tamamlandı",
                PostUpdateAdimDurumu.Basarili, PostUpdateAdimDurumu.Uyari));

        var vm = ViewModel(saga);
        await vm.CalistirAsync();

        vm.UyariVar.Should().BeTrue();
        vm.DevamEtGorunur.Should().BeTrue();
        vm.OtomatikGecis.Should().BeFalse();
        vm.Bloklayici.Should().BeFalse();
    }

    [Fact]
    public async Task ViewModel_UygulamaHatasi_KapatGorunur_AutoYok()
    {
        var saga = new Mock<IPostUpdateDogrulamaService>();
        saga.Setup(s => s.CalistirAsync(It.IsAny<IProgress<PostUpdateAdimSonucu>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sonuc(PostUpdateSonucTuru.UygulamaBasarisiz, "Uygulama güncellenemedi", PostUpdateAdimDurumu.Hata));

        var vm = ViewModel(saga);
        await vm.CalistirAsync();

        vm.UygulamaHatasiGorunur.Should().BeTrue();
        vm.DevamEtGorunur.Should().BeFalse();
        vm.SistemHatasiGorunur.Should().BeFalse();
        vm.Bloklayici.Should().BeFalse();
        vm.OtomatikGecis.Should().BeFalse();
        vm.SonucBaslik.Should().Be("Uygulama güncellenemedi");
    }

    [Fact]
    public async Task ViewModel_SistemHatasi_Bloklar_YonetimGorunur()
    {
        var saga = new Mock<IPostUpdateDogrulamaService>();
        saga.Setup(s => s.CalistirAsync(It.IsAny<IProgress<PostUpdateAdimSonucu>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Sonuc(PostUpdateSonucTuru.SistemBasarisiz, "Sistem veritabanı güncellenemedi", PostUpdateAdimDurumu.Hata));

        var vm = ViewModel(saga);
        await vm.CalistirAsync();

        vm.SistemHatasiGorunur.Should().BeTrue();
        vm.DevamEtGorunur.Should().BeFalse();
        vm.Bloklayici.Should().BeTrue();
        vm.DevamEdilebilir.Should().BeFalse();
        vm.OtomatikGecis.Should().BeFalse();
        vm.SonucAciklama.Should().Be("özet");
    }
}
