using Microsoft.Extensions.Logging;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;

namespace MuhasibPro.Business.Services.SistemServices.LogServices;

/// <summary>
/// Çalışma-zamanı dosya-log eşiği (Faz 6.82).
/// Tek cümle: dosya logger'ının minimum seviyesini tutar; logger host kurulumunda DI'dan önce
/// oluşturulduğu için eşik statik alanda yaşar ve <see cref="FileLoggerProvider"/> buradan okur.
/// </summary>
public class LogSeviyesiYoneticisi : ILogSeviyesiYoneticisi
{
    private static LogLevel _esik = LogLevel.Information;

    /// <summary>Dosya logger'ının okuduğu geçerli eşik (statik — logger DI'dan önce kurulur).</summary>
    public static LogLevel GecerliEsik => _esik;

    public LogLevel Esik => _esik;

    public bool AyrintiliAktif => _esik <= LogLevel.Debug;

    public void AyrintiliAyarla(bool acik) => _esik = acik ? LogLevel.Debug : LogLevel.Information;
}
