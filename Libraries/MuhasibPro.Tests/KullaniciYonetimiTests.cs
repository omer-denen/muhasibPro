using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.85 K2-REDESIGN + K3: yeni kullanıcı yönetimi VM'leri (liste/detay/orkestratör/izin matrisi).</summary>
public class KullaniciYonetimiTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var ortak = new Mock<ICommonServices>();
        var ctx = new Mock<IContextService>();
        ctx.SetupGet(c => c.IsMainView).Returns(false);
        ctx.Setup(c => c.RunAsync(It.IsAny<Action>()))
           .Returns((Action a) => { a(); return Task.CompletedTask; });
        ortak.SetupGet(o => o.ContextService).Returns(ctx.Object);
        ortak.SetupGet(o => o.MessageService).Returns(new Mock<IMessageService>().Object);
        ortak.SetupGet(o => o.DialogService).Returns(new Mock<IDialogService>().Object);
        ortak.SetupGet(o => o.NavigationService).Returns(new Mock<INavigationService>().Object);
        ortak.SetupGet(o => o.NotificationService).Returns(new Mock<INotificationService>().Object);
        var durum = new Mock<IStatusMessageService>();
        durum.Setup(s => s.ExecuteActionAsync(
                It.IsAny<Func<Task>>(), It.IsAny<string>(), It.IsAny<StatusMessageType>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int?>()))
             .Returns((Func<Task> a, string _, StatusMessageType __, string ___, string ____, bool _____, int? ______) => a());
        ortak.SetupGet(o => o.StatusMessageService).Returns(durum.Object);
        ortak.SetupGet(o => o.StatusBarService).Returns(new Mock<IStatusBarService>().Object);
        var log = new Mock<ILogService>();
        log.SetupGet(l => l.SistemLogService).Returns(new Mock<ISistemLogService>().Object);
        log.SetupGet(l => l.AppLogService).Returns(new Mock<IAppLogService>().Object);
        ortak.SetupGet(o => o.LogService).Returns(log.Object);
        return ortak;
    }

    private static IAuthenticationService Kimlik(KullaniciRolTip? rolTip, long id = 5)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(rolTip.HasValue);
        auth.SetupGet(a => a.GetCurrentUserId).Returns(rolTip.HasValue ? id : -1);
        auth.SetupGet(a => a.CurrentAccount).Returns(rolTip.HasValue
            ? new HesapModel
            {
                KullaniciId = id,
                KullaniciModel = new KullaniciModel
                {
                    Rol = new KullaniciRolModel { RolTip = rolTip.Value }
                }
            }
            : null!);
        return auth.Object;
    }

    private static KullaniciRolModel Rol(KullaniciRolTip tip) => new()
    {
        Id = tip == KullaniciRolTip.Yönetici ? KullaniciRolSabitleri.YoneticiRolId : KullaniciRolSabitleri.KullaniciRolId,
        RolAdi = tip == KullaniciRolTip.Yönetici ? "Yönetici" : "Kullanıcı",
        RolTip = tip
    };

    private static Mock<IFirmaWithMaliDonemSelectedService> Secim(long firmaId = 7)
    {
        var secim = new Mock<IFirmaWithMaliDonemSelectedService>();
        secim.SetupGet(s => s.SelectedFirma).Returns(new FirmaModel { Id = firmaId, KisaUnvani = "Test Firma" });
        return secim;
    }

    private static KullaniciDetailsViewModel Detay(
        out Mock<IKullaniciService> servis,
        KullaniciRolTip? izinRol = null)
    {
        servis = new Mock<IKullaniciService>();
        servis.Setup(s => s.GetRollerAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciRolModel>>(
                new List<KullaniciRolModel> { Rol(KullaniciRolTip.Yönetici), Rol(KullaniciRolTip.Kullanici) }, "ok"));
        return new KullaniciDetailsViewModel(
            OrtakServisler().Object,
            new Mock<IFilePickerService>().Object,
            servis.Object,
            Secim().Object);
    }

    // ── Liste VM ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Liste_LoadAsync_FirmaIle_Doldurur()
    {
        var servis = new Mock<IKullaniciService>();
        servis.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>
            {
                new() { Id = 1, KullaniciAdi = "a", Adi = "A", Soyadi = "B", RolId = KullaniciRolSabitleri.KullaniciRolId },
                new() { Id = 2, KullaniciAdi = "b", Adi = "C", Soyadi = "D", RolId = KullaniciRolSabitleri.YoneticiRolId }
            }, "ok"));

        var vm = new KullaniciListViewModel(OrtakServisler().Object, servis.Object);

        await vm.LoadAsync(7);

        vm.ItemsCount.Should().Be(2);
        vm.Items.Should().HaveCount(2);
        vm.SelectedItem.Id.Should().Be(1);
        servis.Verify(s => s.GetKullanicilarWithRolAsync(7), Times.AtLeastOnce);
    }

    // ── Detay VM ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Detay_Yeni_KullaniciAdiVeAdZorunlu()
    {
        var vm = Detay(out var servis);
        await vm.LoadAsync(new KullaniciDetailsArgs { FirmaId = 7 });
        vm.ItemIsNew.Should().BeTrue();
        vm.IsEditMode.Should().BeTrue();

        vm.EditableItem.Adi = string.Empty;
        var sonuc = vm.ValidateModel(vm.EditableItem);
        sonuc.IsValid.Should().BeFalse();
        sonuc.Errors.Keys.Should().Contain("Adi");
    }

    [Fact]
    public async Task Detay_Yeni_Create_FirmaVeRolIle_ResmiKalicilastirir()
    {
        var vm = Detay(out var servis);
        servis.Setup(s => s.CreateKullaniciAsync(It.IsAny<KullaniciModel>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));

        await vm.LoadAsync(new KullaniciDetailsArgs { FirmaId = 7 });
        vm.EditableItem.KullaniciAdi = "yeni";
        vm.EditableItem.Adi = "Ad";
        vm.EditableItem.Soyadi = "Soyad";
        vm.EditableItem.Resim = new byte[] { 1, 2, 3 };
        vm.EditableItem.ResimOnizleme = new byte[] { 4, 5 };
        vm.Sifre = "gizli-123";
        vm.SifreTekrar = "gizli-123";
        vm.SeciliRol = vm.Roller.First(r => r.RolTip == KullaniciRolTip.Yönetici);

        await vm.SaveAsync();

        servis.Verify(s => s.CreateKullaniciAsync(
            It.Is<KullaniciModel>(m => m.KullaniciAdi == "yeni" && m.Adi == "Ad"
                && m.Resim != null && m.ResimOnizleme != null),
            "gizli-123", 7, KullaniciRolSabitleri.YoneticiRolId), Times.Once);
    }

    [Fact]
    public async Task Detay_Yeni_SifreUyusmazsa_Reddedilir()
    {
        var vm = Detay(out var servis);
        await vm.LoadAsync(new KullaniciDetailsArgs { FirmaId = 7 });
        vm.EditableItem.KullaniciAdi = "yeni";
        vm.EditableItem.Adi = "Ad";
        vm.Sifre = "bir-iki-uc";
        vm.SifreTekrar = "farkli";

        await vm.SaveAsync();

        servis.Verify(s => s.CreateKullaniciAsync(It.IsAny<KullaniciModel>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Detay_Mevcut_UpdateVeRolAta_Cagrilir()
    {
        var vm = Detay(out var servis);
        servis.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>
            {
                new() { Id = 44, KullaniciAdi = "mevcut", Adi = "Ad", Soyadi = "Soyad", AktifMi = true, RolId = KullaniciRolSabitleri.KullaniciRolId }
            }, "ok"));
        servis.Setup(s => s.UpdateKullaniciAsync(It.IsAny<KullaniciModel>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));
        servis.Setup(s => s.RolAtaAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<int>(1, "ok"));

        await vm.LoadAsync(new KullaniciDetailsArgs { KullaniciId = 44, FirmaId = 7 });

        vm.ItemIsNew.Should().BeFalse();
        vm.SeciliRol.RolTip.Should().Be(KullaniciRolTip.Kullanici);

        await vm.SaveAsync();

        servis.Verify(s => s.UpdateKullaniciAsync(It.Is<KullaniciModel>(m => m.Id == 44)), Times.Once);
        servis.Verify(s => s.RolAtaAsync(44, 7, KullaniciRolSabitleri.KullaniciRolId), Times.Once);
    }

    // ── İzin matrisi VM (K3) ────────────────────────────────────────────────

    [Fact]
    public async Task IzinMatrisi_KullaniciSecilir_AtanmisVeKullanilabilirAyrilir()
    {
        var rolYetki = new Mock<IRolYetkiService>();
        rolYetki.Setup(s => s.GetMatrisAsync(KullaniciRolSabitleri.KullaniciRolId)).ReturnsAsync(
            new SuccessApiDataResponse<List<RolIzinModel>>(new List<RolIzinModel>
            {
                new() { Izin = Permission.Cari_Goruntule, Kategori = "Cari", Ad = "Görüntüle", Secili = true },
                new() { Izin = Permission.Cari_Ekle, Kategori = "Cari", Ad = "Ekle" },
                new() { Izin = Permission.Rapor_Goruntule, Kategori = "Raporlama", Ad = "Görüntüle" }
            }, "ok"));

        var kullanici = new Mock<IKullaniciService>();
        kullanici.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>
            {
                new()
                {
                    Id = 44,
                    Adi = "Test",
                    Soyadi = "Kullanici",
                    RolId = KullaniciRolSabitleri.KullaniciRolId,
                    Rol = new KullaniciRolModel { RolAdi = "Kullanıcı", RolTip = KullaniciRolTip.Kullanici }
                }
            }, "ok"));

        var vm = new KullaniciRolYetkiViewModel(OrtakServisler().Object, rolYetki.Object, kullanici.Object);

        await vm.LoadAsync(7);

        vm.Moduller.Should().HaveCount(2);
        vm.SeciliModul.Kategori.Should().Be("Cari");
        vm.SeciliModul.Izinler.Should().HaveCount(2);
        vm.SeciliModul.TumuSecili.Should().BeFalse();
        vm.YoneticiMi.Should().BeFalse();
        vm.Duzenlenebilir.Should().BeTrue();

        // Tümü seçilince modülün tüm aksiyonları işaretlenir.
        vm.SeciliModul.TumuSecili = true;
        vm.SeciliModul.Izinler.Should().OnlyContain(i => i.Secili);
    }

    [Fact]
    public async Task IzinMatrisi_YoneticiRolu_SaltOkunur()
    {
        var rolYetki = new Mock<IRolYetkiService>();
        rolYetki.Setup(s => s.GetMatrisAsync(KullaniciRolSabitleri.YoneticiRolId)).ReturnsAsync(
            new SuccessApiDataResponse<List<RolIzinModel>>(new List<RolIzinModel>
            {
                new() { Izin = Permission.Cari_Goruntule, Kategori = "Cari", Ad = "Görüntüle", Secili = true },
                new() { Izin = Permission.Cari_Ekle, Kategori = "Cari", Ad = "Ekle", Secili = true }
            }, "ok"));

        var kullanici = new Mock<IKullaniciService>();
        kullanici.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>
            {
                new()
                {
                    Id = 1,
                    Adi = "Seed",
                    Soyadi = "Yonetici",
                    RolId = KullaniciRolSabitleri.YoneticiRolId,
                    Rol = new KullaniciRolModel { RolAdi = "Yönetici", RolTip = KullaniciRolTip.Yönetici }
                }
            }, "ok"));

        var vm = new KullaniciRolYetkiViewModel(OrtakServisler().Object, rolYetki.Object, kullanici.Object);

        await vm.LoadAsync(7);

        vm.YoneticiMi.Should().BeTrue();
        vm.Duzenlenebilir.Should().BeFalse();
        vm.Moduller.Should().HaveCount(1);
        vm.Moduller[0].Izinler.Should().HaveCount(2);
        vm.Moduller[0].TumuSecili.Should().BeTrue();
    }

    // ── Orkestratör ─────────────────────────────────────────────────────────

    private static KullaniciYonetimiViewModel Orkestrator(
        out Mock<IKullaniciService> servis,
        KullaniciRolTip? rolTip = KullaniciRolTip.Yönetici)
    {
        servis = new Mock<IKullaniciService>();
        servis.Setup(s => s.GetRollerAsync()).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciRolModel>>(
                new List<KullaniciRolModel> { Rol(KullaniciRolTip.Kullanici) }, "ok"));
        servis.Setup(s => s.GetKullanicilarWithRolAsync(7)).ReturnsAsync(
            new SuccessApiDataResponse<List<KullaniciModel>>(new List<KullaniciModel>(), "ok"));

        var rolYetki = new Mock<IRolYetkiService>();
        rolYetki.Setup(s => s.GetMatrisAsync(It.IsAny<long>())).ReturnsAsync(
            new SuccessApiDataResponse<List<RolIzinModel>>(new List<RolIzinModel>(), "ok"));

        return new KullaniciYonetimiViewModel(
            OrtakServisler().Object,
            servis.Object,
            rolYetki.Object,
            Kimlik(rolTip),
            Secim().Object,
            new Mock<IFilePickerService>().Object);
    }

    [Fact]
    public async Task Orkestrator_YoneticiDegil_Yetkisiz_ServisCagrilmaz()
    {
        var vm = Orkestrator(out var servis, KullaniciRolTip.Kullanici);

        await vm.LoadAsync();

        vm.Yetkisiz.Should().BeTrue();
        servis.Verify(s => s.GetKullanicilarWithRolAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Orkestrator_Yonetici_FirmaAdiVeListeYuklenir()
    {
        var vm = Orkestrator(out var servis);

        await vm.LoadAsync();

        vm.Yetkisiz.Should().BeFalse();
        vm.FirmaAdi.Should().Be("Test Firma");
        vm.IsYukleniyor.Should().BeFalse();
        servis.Verify(s => s.GetKullanicilarWithRolAsync(7), Times.Exactly(2));
    }
}
