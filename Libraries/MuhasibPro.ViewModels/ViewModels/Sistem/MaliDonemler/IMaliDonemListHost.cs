using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

/// <summary>Dönem listesi barındıran host'ların ortak yüzü (FirmaShell + MaliDonem yönetim penceresi).</summary>
public interface IMaliDonemListHost
{
    MaliDonemListViewModel MaliDonemList { get; }
    FirmaModel SelectedFirma { get; }
}
