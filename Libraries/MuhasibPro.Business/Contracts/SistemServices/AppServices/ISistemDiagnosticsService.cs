namespace MuhasibPro.Business.Contracts.SistemServices.AppServices;

public interface ISistemDiagnosticsService
{
    Task<int> GetKullaniciCountAsync();
    Task<int> GetFirmaCountAsync();
    Task<bool> CanConnectAsync();
    Task<(int FirmaCount, int DonemCount, string IlkFirmaKisaUnvani)> GetFirmaMaliDonemStatsAsync();
    Task<(int KfrCount, int PermCount, string Sample)> GetKullaniciFirmaRolStatsAsync();
}
