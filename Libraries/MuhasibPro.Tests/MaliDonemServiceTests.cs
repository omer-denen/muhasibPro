using FluentAssertions;
using Moq;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.SistemServices.AppServices;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.Contracts.Repository.SistemRepos;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Entities.SistemEntity;
using System.Linq.Expressions;

namespace MuhasibPro.Tests;

/// <summary>Faz 6.71/1: MaliDonem kayıt servisi davranış mührü (kod değiştirmeden).
/// Gerçek MaliDonemService + mock repo/UoW/auth: guard'lar, sayımın 0'daki hata dalı,
/// oluşturma/güncelleme/geri-alma/silme ve firma-filtreli listeleme.</summary>
public class MaliDonemServiceTests
{
    private sealed class DonemKurulum
    {
        public Mock<IMaliDonemRepository> Repo = new();
        public Mock<IUnitOfWork<SistemDbContext>> Uow = new();
        public Mock<IAuthenticationService> Auth = new();
        public Mock<IFirmaService> Firmalar = new();
        public Mock<ILogService> Log = new();
        public Mock<IEntityRegistrySettingsProvider> Ayar = new();

        public DonemKurulum(long kullaniciId = 1)
        {
            Auth.SetupGet(a => a.GetCurrentUserId).Returns(kullaniciId);
            Log.SetupGet(l => l.SistemLogService).Returns(Mock.Of<ISistemLogService>());
            Ayar.Setup(e => e.GetAsync(It.IsAny<long>()))
                .ReturnsAsync(new Domain.Models.EntityRegistrySettings());
        }

        public MaliDonemService Servis() => new(
            Repo.Object, Log.Object, Uow.Object, Auth.Object,
            Firmalar.Object, Mock.Of<IBitmapToolsService>(), Ayar.Object);

        public static MaliDonem Satir(long id = 5, long firmaId = 7, int yil = 2026) => new()
        {
            Id = id,
            FirmaId = firmaId,
            MaliYil = yil,
            DatabaseName = $"db-F{firmaId}_{yil}"
        };
    }

    #region Okuma

    [Fact]
    public async Task GetById_Bulundu()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(5)).ReturnsAsync(DonemKurulum.Satir());

        var sonuc = await kur.Servis().GetByMaliDonemIdAsync(5);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.DatabaseName.Should().Be("db-F7_2026");
    }

    [Fact]
    public async Task GetById_Bulunamadi()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(99)).ReturnsAsync((MaliDonem)null!);

        var sonuc = await kur.Servis().GetByMaliDonemIdAsync(99);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Page_Listeler()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetMaliDonemlerAsync(0, 10, It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync(new List<MaliDonem> { DonemKurulum.Satir(5), DonemKurulum.Satir(6, yil: 2027) });

        var sonuc = await kur.Servis().GetMaliDonemlerPageAsync(0, 10, new DataRequest<MaliDonem>());

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Page_RepoNull_HataVerir()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetMaliDonemlerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DataRequest<MaliDonem>>()))
            .ReturnsAsync((IList<MaliDonem>)null!);

        var sonuc = await kur.Servis().GetMaliDonemlerPageAsync(0, 10, new DataRequest<MaliDonem>());

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Count_Sifir_HataVerir()
    {
        // Mevcut davranış mührü: 0 kayıt Error dalına düşer (liste boşken sayaç hata verir).
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetMaliDonemlerCountAsync(It.IsAny<DataRequest<MaliDonem>>())).ReturnsAsync(0);

        var sonuc = await kur.Servis().GetMaliDonemlerCountAsync(new DataRequest<MaliDonem>());

        sonuc.Success.Should().BeFalse();
        sonuc.Data.Should().Be(0);
    }

    [Fact]
    public async Task Count_DegerDoner()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetMaliDonemlerCountAsync(It.IsAny<DataRequest<MaliDonem>>())).ReturnsAsync(3);

        var sonuc = await kur.Servis().GetMaliDonemlerCountAsync(new DataRequest<MaliDonem>());

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().Be(3);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsAny_Passthrough(bool deger)
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.IsMaliDonemAnyAsync()).ReturnsAsync(deger);

        (await kur.Servis().IsMaliDonemAnyAsync()).Should().Be(deger);
    }

    [Fact]
    public async Task Exists_Varsa_True()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<MaliDonem, bool>>>()))
            .ReturnsAsync(DonemKurulum.Satir());

        (await kur.Servis().IsMaliDonemExistsAsync(7, 2026)).Should().BeTrue();
    }

    [Fact]
    public async Task Exists_Yoksa_False()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<MaliDonem, bool>>>()))
            .ReturnsAsync((MaliDonem)null!);

        (await kur.Servis().IsMaliDonemExistsAsync(7, 2030)).Should().BeFalse();
    }

    #endregion

    #region Oluşturma

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_GecersizFirmaId_HataVerir(long firmaId)
    {
        var kur = new DonemKurulum();

        var sonuc = await kur.Servis().CreateNewMaliDonemForFirmaAsync(firmaId);

        sonuc.Success.Should().BeFalse();
        kur.Firmalar.Verify(f => f.GetByFirmaIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Create_FirmaYok_HataVerir()
    {
        var kur = new DonemKurulum();
        kur.Firmalar.Setup(f => f.GetByFirmaIdAsync(7)).ReturnsAsync(
            new Domain.Utilities.Responses.ErrorApiDataResponse<FirmaModel>(data: null, message: "yok"));

        var sonuc = await kur.Servis().CreateNewMaliDonemForFirmaAsync(7);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("Firma bulunamadı");
    }

    [Fact]
    public async Task Create_Basarili_VarsayilanDurumAyarlanir()
    {
        var kur = new DonemKurulum();
        kur.Firmalar.Setup(f => f.GetByFirmaIdAsync(7)).ReturnsAsync(
            new Domain.Utilities.Responses.SuccessApiDataResponse<FirmaModel>(new FirmaModel { Id = 7 }, "ok"));

        var sonuc = await kur.Servis().CreateNewMaliDonemForFirmaAsync(7);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.FirmaId.Should().Be(7);
        sonuc.Data.TenantDetails.Should().NotBeNull();
        sonuc.Data.TenantDetails.Durum.Should().Be(Domain.Enum.DonemDurum.Acik);
    }

    #endregion

    #region Güncelleme + geri alma

    [Fact]
    public async Task Update_Kullanicisiz_HataVerir()
    {
        var kur = new DonemKurulum(kullaniciId: 0);

        var sonuc = await kur.Servis().UpdateMaliDonemAsync(new MaliDonemModel { Id = 5 });

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("kullanıcı bilgisi");
    }

    [Fact]
    public async Task Update_Null_HataVerir()
    {
        var kur = new DonemKurulum();

        var sonuc = await kur.Servis().UpdateMaliDonemAsync(null!);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("boş olamaz");
    }

    [Fact]
    public async Task Update_Basarili()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(5)).ReturnsAsync(DonemKurulum.Satir());
        kur.Uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var sonuc = await kur.Servis().UpdateMaliDonemAsync(new MaliDonemModel { Id = 5, FirmaId = 7, MaliYil = 2026 });

        sonuc.Success.Should().BeTrue();
        sonuc.Message.Should().Contain("güncellendi");
        kur.Repo.Verify(r => r.UpdateMaliDonemAsync(It.IsAny<MaliDonem>()), Times.Once);
    }

    [Fact]
    public async Task Update_Kaydetmedi_HataVerir()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(5)).ReturnsAsync(DonemKurulum.Satir());
        kur.Uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

        var sonuc = await kur.Servis().UpdateMaliDonemAsync(new MaliDonemModel { Id = 5 });

        sonuc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Restore_Kullanicisiz_HataVerir()
    {
        var kur = new DonemKurulum(kullaniciId: 0);

        var sonuc = await kur.Servis().RestoreMaliDonemAsync(new MaliDonemModel { Id = 5 });

        sonuc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Restore_Basarili()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(5)).ReturnsAsync(DonemKurulum.Satir());
        kur.Uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var sonuc = await kur.Servis().RestoreMaliDonemAsync(new MaliDonemModel { Id = 5, FirmaId = 7, MaliYil = 2026 });

        sonuc.Success.Should().BeTrue();
        kur.Repo.Verify(r => r.RestoreMaliDonemAsync(It.IsAny<MaliDonem>()), Times.Once);
    }

    #endregion

    #region Silme + firma listesi

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public async Task Delete_GecersizId_HataVerir(long id)
    {
        var kur = new DonemKurulum();

        var sonuc = await kur.Servis().DeleteMaliDonemAsync(id);

        sonuc.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_Bulunamadi_HataVerir()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(99)).ReturnsAsync((MaliDonem)null!);

        var sonuc = await kur.Servis().DeleteMaliDonemAsync(99);

        sonuc.Success.Should().BeFalse();
        sonuc.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Delete_Basarili()
    {
        var kur = new DonemKurulum();
        kur.Repo.Setup(r => r.GetByMaliDonemIdAsync(5)).ReturnsAsync(DonemKurulum.Satir());
        kur.Uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var sonuc = await kur.Servis().DeleteMaliDonemAsync(5);

        sonuc.Success.Should().BeTrue();
        sonuc.Message.Should().Contain("silme işlemi başarılı");
    }

    [Fact]
    public async Task WithFirmaId_SadeceOfirmaninDonemleri()
    {
        var kur = new DonemKurulum();
        var satirlar = new List<MaliDonem> { DonemKurulum.Satir(5, 7), DonemKurulum.Satir(9, 8) };
        kur.Repo.Setup(r => r.GetQuery(It.IsAny<DataRequest<MaliDonem>>())).Returns(satirlar.AsQueryable());

        var sonuc = await kur.Servis().GetMaliDonemlerWithFirmaId(new DataRequest<MaliDonem>(), 7);

        sonuc.Success.Should().BeTrue();
        sonuc.Data.Should().ContainSingle(m => m.FirmaId == 7);
    }

    #endregion
}
