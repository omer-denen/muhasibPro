using System.Reflection;
using FluentAssertions;
using MuhasibPro.Domain.Entities.SistemEntity;

namespace MuhasibPro.Tests;

/// <summary>
/// Çekirdek kilidi: Domain yapısı bozulursa test kırmızı.
/// ViewModel sadece alttan gelen modele göre bind ekler, Domain’i başka model bozamaz.
/// </summary>
public class DomainCoreTests
{
    [Fact]
    public void Kullanici_RolId_Olmamali_KullaniciFirmaRoller_Olmali()
    {
        var type = typeof(Kullanici);
        type.GetProperty("RolId").Should().BeNull("Kullanici.RolId kaldırıldı, yerine KullaniciFirmaRoller var (Oturum 2)");
        type.GetProperty("Rol").Should().BeNull("Kullanici.Rol navigation kaldırıldı");
        type.GetProperty("KullaniciFirmaRoller").Should().NotBeNull();
        type.GetProperty("KullaniciFirmaRoller")!.PropertyType.Should().BeAssignableTo(typeof(ICollection<KullaniciFirmaRol>));
    }

    [Fact]
    public void Firma_KullaniciFirmaRoller_Icermeli()
    {
        var type = typeof(Firma);
        type.GetProperty("KullaniciFirmaRoller").Should().NotBeNull("Firma → KullaniciFirmaRol (Oturum 2)");
        type.GetProperty("MaliDonemler").Should().NotBeNull();
        type.GetProperty("Hesaplar").Should().NotBeNull();
    }

    [Fact]
    public void MaliDonem_Genisletme_Alanlari_Olmali()
    {
        var type = typeof(MaliDonem);
        type.GetProperty("Durum").Should().NotBeNull("DonemDurum eklendi");
        type.GetProperty("DosyaBoyutu").Should().NotBeNull();
        type.GetProperty("SonYedekTarihi").Should().NotBeNull();
        type.GetProperty("ArsivlendiMi").Should().NotBeNull();
        type.GetProperty("DatabaseName").Should().NotBeNull();
        type.GetProperty("FirmaId").Should().NotBeNull();
    }

    [Fact]
    public void KullaniciRol_Yetkiler_Icermeli()
    {
        var type = typeof(KullaniciRol);
        type.GetProperty("Yetkiler").Should().NotBeNull("KullaniciRol.Yetkiler → RolPermission (Oturum 2)");
        type.GetProperty("Yetkiler")!.PropertyType.Should().BeAssignableTo(typeof(ICollection<RolPermission>));
    }

    [Fact]
    public void KullaniciFirmaRol_CompositeKey_Uclu_Alan()
    {
        var type = typeof(KullaniciFirmaRol);
        type.GetProperty("KullaniciId").Should().NotBeNull();
        type.GetProperty("FirmaId").Should().NotBeNull();
        type.GetProperty("RolId").Should().NotBeNull();
        type.GetProperty("Kullanici").Should().NotBeNull();
        type.GetProperty("Firma").Should().NotBeNull();
        type.GetProperty("Rol").Should().NotBeNull();
    }

    [Fact]
    public void Domain_14_Tablo_Entity_Var()
    {
        var asm = typeof(Kullanici).Assembly;
        var entities = asm.GetTypes().Where(t => t.Namespace == "MuhasibPro.Domain.Entities.SistemEntity" && t.IsClass && !t.IsAbstract).Select(t => t.Name).ToHashSet();
        entities.Should().Contain("Kullanici");
        entities.Should().Contain("KullaniciRol");
        entities.Should().Contain("KullaniciFirmaRol");
        entities.Should().Contain("RolPermission");
        entities.Should().Contain("Firma");
        entities.Should().Contain("MaliDonem");
        entities.Should().Contain("AuditLog");
        entities.Should().Contain("Lisans");
        entities.Should().Contain("GlobalAyarlar");
        entities.Should().Contain("OturumKaydi");
        entities.Should().Contain("SistemLog");
        entities.Should().Contain("Hesap");
    }

    [Fact]
    public void Permission_68_Yetki_Icermeli()
    {
        var permCount = Enum.GetValues(typeof(MuhasibPro.Domain.Enum.Permission)).Length;
        permCount.Should().BeGreaterThanOrEqualTo(68, "Permission en az 68 aksiyon bazlı yetki (Oturum 2), 75'e çıktıysa genişleme normal");
    }
}
