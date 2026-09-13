using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.SistemDbYonetim;

public class KurulumKayitViewModel : ViewModelBase
{
    private readonly IKurulumKayitService _kurulumService;
    private readonly IMakineKimligiProvider _makineProvider;

    public KurulumKayitViewModel(
        Business.Contracts.UIServices.CommonServices.ICommonServices commonServices,
        IKurulumKayitService kurulumService,
        IMakineKimligiProvider makineProvider) : base(commonServices)
    {
        _kurulumService = kurulumService;
        _makineProvider = makineProvider;
    }

    private string _kurulumId = "-";
    public string KurulumId { get => _kurulumId; set => Set(ref _kurulumId, value); }

    private string _machineGuid = "-";
    public string MachineGuid { get => _machineGuid; set => Set(ref _machineGuid, value); }

    private string _olusturmaTarihi = "-";
    public string OlusturmaTarihi { get => _olusturmaTarihi; set => Set(ref _olusturmaTarihi, value); }

    public async Task YukleAsync()
    {
        try
        {
            IsBusy = true;
            var kayit = await _kurulumService.GetOrCreateAsync();
            KurulumId = kayit.KurulumId ?? "-";
            MachineGuid = kayit.MachineGuid ?? "-";
            OlusturmaTarihi = kayit.OlusturmaTarihi == default ? "-" : kayit.OlusturmaTarihi.ToString("yyyy-MM-dd HH:mm");

            // Makine kimliği güncelini de göster (registry okuması)
            try
            {
                var currentMachine = await _makineProvider.GetMachineIdAsync();
                if (!string.Equals(currentMachine, MachineGuid, StringComparison.OrdinalIgnoreCase))
                    MachineGuid = $"{MachineGuid} (şu an: {currentMachine})";
            }
            catch { }
        }
        catch
        {
            KurulumId = "-";
            MachineGuid = "-";
            OlusturmaTarihi = "-";
        }
        finally { IsBusy = false; }
    }
}
