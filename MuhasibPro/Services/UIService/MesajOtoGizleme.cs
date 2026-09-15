using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Services.UIService
{
    /// <summary>
    /// Status mesajı otomatik gizleme (tek sorumluluk: süre yönetimi).
    /// Süre verilmezse AppPlatform ayarından (`StatusAutoHideMs`) okunur — hardcoded süre yok.
    /// </summary>
    public sealed class MesajOtoGizleme : IDisposable
    {
        private readonly IAppPlatformSettingsProvider _ayarSaglayici;
        private readonly Action _gizle;
        private CancellationTokenSource _cts;
        private int _nesil;

        public MesajOtoGizleme(IAppPlatformSettingsProvider ayarSaglayici, Action gizle)
        {
            _ayarSaglayici = ayarSaglayici;
            _gizle = gizle;
        }

        /// <summary>null → ayardan varsayılan süre; &lt;= 0 → otomatik gizleme yok; &gt; 0 → saniye.</summary>
        public void Baslat(int? saniye)
        {
            int nesil = ++_nesil;
            IptalIc();

            if (saniye.HasValue)
            {
                if (saniye.Value > 0)
                    Kur(TimeSpan.FromSeconds(saniye.Value), nesil);
                return;
            }

            _ = BaslatVarsayilanAsync(nesil);
        }

        public void Iptal()
        {
            _nesil++;
            IptalIc();
        }

        private async Task BaslatVarsayilanAsync(int nesil)
        {
            int ms = new AppPlatformSettings().StatusAutoHideMs;
            try
            {
                ms = (await _ayarSaglayici.GetAsync()).StatusAutoHideMs;
            }
            catch
            {
                // Ayar okunamazsa model varsayılanı geçerli
            }

            if (nesil != _nesil) return;
            Kur(TimeSpan.FromMilliseconds(ms), nesil);
        }

        private void Kur(TimeSpan sure, int nesil)
        {
            _cts = new CancellationTokenSource();
            _ = GizleAsync(sure, _cts.Token, nesil);
        }

        private async Task GizleAsync(TimeSpan sure, CancellationToken ct, int nesil)
        {
            try
            {
                await Task.Delay(sure, ct);
                if (!ct.IsCancellationRequested && nesil == _nesil)
                    _gizle();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void IptalIc()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch
            {
                // yut — iptal en iyi çaba
            }
            _cts = null;
        }

        public void Dispose() => Iptal();
    }
}
