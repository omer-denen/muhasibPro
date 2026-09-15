using Microsoft.Extensions.Logging;

namespace MuhasibPro.Business.Contracts.SistemServices.LogServices;

/// <summary>
/// Çalışma-zamanı dosya-log eşiği (Faz 6.82 geliştirici aracı).
/// Tek cümle: dosya logger'ının minimum seviyesini okur ve değiştirir.
/// </summary>
public interface ILogSeviyesiYoneticisi
{
    /// <summary>Geçerli minimum seviye (varsayılan <see cref="LogLevel.Information"/>).</summary>
    LogLevel Esik { get; }

    /// <summary>Ayrıntılı (Debug) seviyesi açık mı.</summary>
    bool AyrintiliAktif { get; }

    /// <summary>Ayrıntılı log seviyesini açar/kapatır (true → Debug, false → Information).</summary>
    void AyrintiliAyarla(bool acik);
}
