using FluentAssertions;
using MuhasibPro.Business.Services.SistemServices.DevServices;

namespace MuhasibPro.Tests;

/// <summary>Modül entegrasyon testleri (donanım POST) — boş sağlayıcıda bile tüm modüller için
/// sonuç döner ve exception fırlatmaz; her satır modül+ad taşır.</summary>
public class ModulTestCalistiriciTests
{
    private sealed class BosServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }

    [Fact]
    public async Task Calistir_BosSaglayici_TumModullerSonucDonerVeHataVermez()
    {
        var calistirici = new ModulTestCalistirici(new BosServiceProvider());

        var sonuclar = await calistirici.CalistirAsync();

        sonuclar.Should().NotBeEmpty();
        sonuclar.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.Modul) && !string.IsNullOrWhiteSpace(s.Ad));
        sonuclar.Select(s => s.Modul).Should().Contain(new[] { "Çekirdek", "Sistem Veritabanı", "Tenant Veritabanı", "Güncelleme", "Dev Araçları" });
        // Boş sağlayıcıda hiçbir servis çözülemez → hiçbir test başarılı olmamalı.
        sonuclar.Should().OnlyContain(s => !s.Basarili);
    }
}
