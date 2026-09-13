using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.Tests;

/// <summary>Denetim Masası iskeleti: bölüm durumu + kullanıcı-firma bağlamı.</summary>
public class DenetimMasasiTests
{
    private static Mock<ICommonServices> OrtakServisler()
    {
        var baglam = new Mock<IContextService>();
        baglam.Setup(c => c.RunAsync(It.IsAny<Action>()))
            .Returns((Action a) => { a(); return Task.CompletedTask; });
        var ortak = new Mock<ICommonServices>();
        ortak.SetupGet(o => o.ContextService).Returns(baglam.Object);
        ortak.SetupGet(o => o.MessageService).Returns(Mock.Of<IMessageService>());
        return ortak;
    }

    private static IAuthenticationService Kimlik(long kullaniciId, string ad = null!)
    {
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.CurrentAccount).Returns(new HesapModel
        {
            KullaniciId = kullaniciId,
            KullaniciModel = ad == null ? null! : new KullaniciModel
            {
                KullaniciAdi = ad,
                Rol = new KullaniciRolModel { RolAdi = "Yönetici", RolTip = KullaniciRolTip.Yönetici }
            }
        });
        return auth.Object;
    }

    private static Mock<IFirmaService> FirmaServisi()
    {
        var svc = new Mock<IFirmaService>();
        svc.Setup(s => s.GetFirmalarWithUserId(It.IsAny<DataRequest<Firma>>(), It.IsAny<long>()))
            .ReturnsAsync(new SuccessApiDataResponse<IList<FirmaModel>>(new List<FirmaModel>
            {
                new() { Id = 7, FirmaKodu = "F-0007", KisaUnvani = "Test" },
                new() { Id = 8, FirmaKodu = "F-0008", KisaUnvani = "Test2" }
            }, "ok"));
        return svc;
    }

    [Fact]
    public void VarsayilanBolum_Giris()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        vm.SeciliBolum.Should().Be(AyarBolumu.GirisPaneli);
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.GirisPaneli);
        vm.Gorunum.Should().NotBeNull();
    }

    [Fact]
    public void Menuler_YediBolum_VarsayilanGirisSecili()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        vm.Menuler.Should().HaveCount(7);
        vm.Menuler.Select(m => m.Bolum).Should().ContainInOrder(
            AyarBolumu.GirisPaneli, AyarBolumu.Gorunum, AyarBolumu.Giris, AyarBolumu.Firma,
            AyarBolumu.Veritabani, AyarBolumu.Donem, AyarBolumu.Guncelleme);
        var veritabani = vm.Menuler.Single(m => m.Bolum == AyarBolumu.Veritabani);
        veritabani.AltMenuler.Should().BeEmpty("Veritabanı tek view: Sistem + Dönem grupları sayfa içindedir");
        vm.GorunurMenuler.Should().HaveCount(7, "demo yönetici admin-dev: tümü görünür");
        vm.SeciliMenu.Should().NotBeNull();
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.GirisPaneli);
    }

    [Fact]
    public void SeciliMenu_IkiYonluSenkron()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        vm.SeciliBolum = AyarBolumu.Guncelleme;

        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.Guncelleme);

        vm.SeciliMenu = vm.GorunurMenuler.Single(m => m.Bolum == AyarBolumu.Firma);

        vm.SeciliBolum.Should().Be(AyarBolumu.Firma);
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.Firma);
    }

    [Fact]
    public async Task GorunurMenuleriTazele_SecimiKorur()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object, null!, Kimlik(7));
        vm.SeciliBolum = AyarBolumu.Veritabani;

        await vm.LoadAsync();

        vm.GorunurMenuler.Should().HaveCount(7);
        vm.SeciliBolum.Should().Be(AyarBolumu.Veritabani);
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.Veritabani);
    }

    [Fact]
    public void AramaMetni_MenuyuSuzer()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        vm.AramaMetni = "yedek";

        vm.GorunurMenuler.Should().ContainSingle().Which.Bolum.Should().Be(AyarBolumu.Veritabani);
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.Veritabani);

        vm.AramaMetni = string.Empty;

        vm.GorunurMenuler.Should().HaveCount(7);
    }

    [Fact]
    public void FirmaDonem_Cocuklari_Bagli()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        vm.FirmaKayit.Should().NotBeNull();
        vm.Donem.Should().NotBeNull();
    }

    [Fact]
    public void BolumSecimi_BildirimleDegisir()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);
        var bildirilen = new List<string>();
        vm.PropertyChanged += (_, e) => { if (e.PropertyName != null) bildirilen.Add(e.PropertyName); };

        vm.SeciliBolum = AyarBolumu.Guncelleme;

        vm.SeciliBolum.Should().Be(AyarBolumu.Guncelleme);
        vm.SeciliMenu.Bolum.Should().Be(AyarBolumu.Guncelleme);
        bildirilen.Should().Contain(nameof(DenetimMasasiViewModel.SeciliMenu));
    }

    [Fact]
    public async Task FirmaSayisi_GirisYapmisKullanicidan_Okunur()
    {
        var firma = FirmaServisi();
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, firma.Object, null!, Kimlik(7));

        await vm.LoadAsync();

        firma.Verify(s => s.GetFirmalarWithUserId(It.IsAny<DataRequest<Firma>>(), 7), Times.Once);
        vm.FirmaSayisi.Should().Be(2);
        vm.FirmaSayisiMetni.Should().Be("2 kayıtlı");
    }

    [Fact]
    public async Task FirmaSayisi_GirisYoksa_Sifir()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object);

        await vm.LoadAsync();

        vm.FirmaSayisi.Should().Be(0);
        vm.FirmaSayisiMetni.Should().Be("0 kayıtlı");
    }

    [Fact]
    public async Task KullaniciKarti_HeroSeridi_Dolar()
    {
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object, null!, Kimlik(7, "Korkut Omer"));

        await vm.LoadAsync();

        vm.KullaniciAdi.Should().Be("Korkut Omer");
        vm.KullaniciRolMetni.Should().Be("Yönetici");
        vm.KullaniciInitials.Should().Be("KO");
        vm.ProfilResmi.Should().BeNull("foto yoksa varsayılan avatar");
        vm.FirmaSayisi.Should().Be(2);
    }

    [Fact]
    public async Task ProfilResmi_Varsa_Passthrough()
    {
        var foto = new byte[] { 1, 2, 3 };
        var auth = new Mock<IAuthenticationService>();
        auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        auth.SetupGet(a => a.CurrentAccount).Returns(new HesapModel
        {
            KullaniciId = 7,
            KullaniciModel = new KullaniciModel { KullaniciAdi = "Ada", Resim = foto }
        });
        var vm = new DenetimMasasiViewModel(OrtakServisler().Object, FirmaServisi().Object, null!, auth.Object);

        await vm.LoadAsync();

        vm.ProfilResmi.Should().BeSameAs(foto);
    }
}
