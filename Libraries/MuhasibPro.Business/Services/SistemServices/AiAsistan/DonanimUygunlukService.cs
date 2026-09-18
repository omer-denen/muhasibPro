using MuhasibPro.Business.Contracts.SistemServices.AiAsistan;

namespace MuhasibPro.Business.Services.SistemServices.AiAsistan
{
    /// <summary>D2-D3: donanım ölçümü (GC/DriveInfo/Environment — platform bağımsız) + saf hüküm fonksiyonu.</summary>
    public sealed class DonanimUygunlukService : IDonanimUygunlukService
    {
        /// <summary>Uygun eşiği (kullanılabilir RAM ≥ 8 GB).</summary>
        public const long UygunEsikMb = 8L * 1024;

        /// <summary>Sınırda eşiği (kullanılabilir RAM 6-8 GB; altı yetersiz).</summary>
        public const long SinirdaEsikMb = 6L * 1024;

        public DonanimBilgisiDto Olc()
        {
            long ramMb;
            try
            {
                ramMb = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
            }
            catch
            {
                ramMb = -1;
            }

            long diskMb = -1;
            try
            {
                var kok = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                if (!string.IsNullOrEmpty(kok))
                    diskMb = new DriveInfo(kok).AvailableFreeSpace / 1024 / 1024;
            }
            catch
            {
                diskMb = -1;
            }

            return new DonanimBilgisiDto
            {
                KullanilabilirRamMb = ramMb,
                BosDiskMb = diskMb,
                IslemciCekirdek = Environment.ProcessorCount
            };
        }

        public DonanimHukmu Degerlendir(DonanimBilgisiDto bilgi)
        {
            if (bilgi is null)
                throw new ArgumentNullException(nameof(bilgi));
            if (bilgi.KullanilabilirRamMb < 0)
                return DonanimHukmu.Yetersiz;
            if (bilgi.KullanilabilirRamMb >= UygunEsikMb)
                return DonanimHukmu.Uygun;
            if (bilgi.KullanilabilirRamMb >= SinirdaEsikMb)
                return DonanimHukmu.Sinirda;
            return DonanimHukmu.Yetersiz;
        }
    }
}
