using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.SistemServices.Authentication;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

/// <summary>Faz 6.85 K2-REDESIGN: Kullanıcı Yönetimi orkestratörü (composition).
/// Tek cümle: liste + detay + rol/izin matrisi VM'lerini barındırır; seçim/kapı/navigasyonu yönetir.</summary>
public class KullaniciYonetimiViewModel : ViewModelBase
{
    private readonly IAuthenticationService _auth;
    private readonly IFirmaWithMaliDonemSelectedService _selection;
    private long _firmaId;

    public KullaniciYonetimiViewModel(
        ICommonServices commonServices,
        IKullaniciService kullaniciService,
        IRolYetkiService rolYetkiService,
        IAuthenticationService auth,
        IFirmaWithMaliDonemSelectedService selection,
        IFilePickerService filePickerService) : base(commonServices)
    {
        _auth = auth;
        _selection = selection;

        List = new KullaniciListViewModel(commonServices, kullaniciService);
        Details = new KullaniciDetailsViewModel(commonServices, filePickerService, kullaniciService, selection);
        RolYetki = new KullaniciRolYetkiViewModel(commonServices, rolYetkiService, kullaniciService);
    }

    public KullaniciListViewModel List { get; }
    public KullaniciDetailsViewModel Details { get; }
    public KullaniciRolYetkiViewModel RolYetki { get; }

    private string _firmaAdi = string.Empty;
    public string FirmaAdi { get => _firmaAdi; set => Set(ref _firmaAdi, value); }

    private bool _isYukleniyor;
    public bool IsYukleniyor { get => _isYukleniyor; set => Set(ref _isYukleniyor, value); }

    private bool _yetkisiz;
    public bool Yetkisiz { get => _yetkisiz; set => Set(ref _yetkisiz, value); }

    public string YetkiMesaji => "Bu ekranı yalnızca yönetici kullanabilir.";

    private int _seciliSekme;
    public int SeciliSekme { get => _seciliSekme; set => Set(ref _seciliSekme, value); }

    public ICommand YenileCommand => new AsyncRelayCommand(LoadAsync);

    public async Task LoadAsync()
    {
        if (!AyarYetkiDenetimi.KullaniciYoneticiMi(_auth))
        {
            Yetkisiz = true;
            return;
        }

        Yetkisiz = false;
        IsYukleniyor = true;
        try
        {
            var firma = _selection.SelectedFirma;
            _firmaId = firma?.Id ?? 0;
            FirmaAdi = string.IsNullOrWhiteSpace(firma?.KisaUnvani) ? "Firma seçili değil" : firma!.KisaUnvani;

            await Details.RolleriYukleAsync();
            await List.LoadAsync(_firmaId);
            await RolYetki.LoadAsync(_firmaId);

            if (List.SelectedItem == null)
                Details.Item = null;
        }
        catch (Exception ex)
        {
            StatusError("Kullanıcı yönetimi yüklenirken beklenmeyen hata");
            await LogSistemExceptionAsync("Kullanıcı Yönetimi", "Yükle", ex);
        }
        finally
        {
            IsYukleniyor = false;
            NotifyPropertyChanged(nameof(ListeBos));
        }
    }

    public bool ListeBos => !IsYukleniyor && List.ItemsCount == 0;

    public void Unload()
    {
        Details.Unload();
    }

    public void Subscribe()
    {
        MessageService.Subscribe<KullaniciListViewModel>(this, OnMessage);
        List.Subscribe();
        Details.Subscribe();
    }

    public void Unsubscribe()
    {
        MessageService.Unsubscribe(this);
        List.Unsubscribe();
        Details.Unsubscribe();
    }

    private async void OnMessage(KullaniciListViewModel viewModel, string message, object args)
    {
        if (viewModel == List && message == EntityEvents.ItemSelected)
        {
            await ContextService.RunAsync(OnItemSelected);
        }
    }

    public void OnItemSelected()
    {
        if (Details.IsEditMode)
        {
            StatusReady();
            Details.CancelEdit();
        }

        var selected = List.SelectedItem;
        if (!List.IsMultipleSelection && selected != null && !selected.IsEmpty)
        {
            Details.Item = selected;
            Details.SenkronizeSeciliRol();
        }
        else
        {
            Details.Item = null;
        }
    }
}
