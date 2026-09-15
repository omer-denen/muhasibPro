using MuhasibPro.Business.DTOModel.DevModel;

namespace MuhasibPro.Business.Contracts.SistemServices.DevServices;

/// <summary>
/// Geliştirici araçları (yalnız DEBUG) — kimlik/transfer + teşhis operasyonları.
/// Tek cümle: kurulum kimliği onarım/sıfırlama, transfer taraması, şema damgası ve log seviyesi işlerini yürütür.
/// (Kurulum modülünden ayrı: tenant/sistem servislerine yaslandığı için mimari bekçi kapsamı dışında.)
/// </summary>
public interface IDevAraclariService
{
    /// <summary>Kurulum kimliği + tenant şema damgaları + yollar (salt-okunur).</summary>
    Task<DevAracDurumuModel> DurumOkuAsync();

    /// <summary>Makinesi aynı olan dönem damgalarını güncel kurulum kimliğine eşitler (onarım).</summary>
    Task<DevAracSonucuModel> KimligiOnarAsync();

    /// <summary>Yeni kurulum kimliği üretir ve bu makinedeki dönem damgalarını bu kimliğe göre yeniler (sıfırlama + damga).</summary>
    Task<DevAracSonucuModel> KimligiSifirlaAsync();

    /// <summary>Taşınmış-veri taramasını elle tetikler (sonuç özeti döner).</summary>
    Task<DevAracSonucuModel> TransferTaramasiAsync();

    /// <summary>Ayrıntılı (Debug) dosya-log seviyesini açar/kapatır.</summary>
    Task<DevAracSonucuModel> AyrintiliLogAyarlaAsync(bool acik);
}
