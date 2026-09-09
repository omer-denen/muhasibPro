using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Settings;

public class ModuleLicenseViewModel : ViewModelBase
{
    private readonly IModuleLicenseService _moduleLicenseService;
    private readonly IAuthenticationService _authenticationService;
    private bool _isLoading;
    private string _statusMessage = string.Empty;

    public ModuleLicenseViewModel(
        ICommonServices commonServices,
        IModuleLicenseService moduleLicenseService,
        IAuthenticationService authenticationService)
        : base(commonServices)
    {
        _moduleLicenseService = moduleLicenseService;
        _authenticationService = authenticationService;

        Modules = new ObservableCollection<ModuleItemViewModel>();
        LoadModulesCommand = new RelayCommand(async () => await LoadModulesAsync());
        ToggleModuleCommand = new RelayCommand<ModuleItemViewModel>(async (item) => await ToggleModuleAsync(item));
        SaveAllCommand = new RelayCommand(async () => await SaveAllAsync());
    }

    public ObservableCollection<ModuleItemViewModel> Modules { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => Set(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }

    public ICommand LoadModulesCommand { get; }
    public ICommand ToggleModuleCommand { get; }
    public ICommand SaveAllCommand { get; }

    public async Task LoadAsync(ShellArgs args)
    {
        await LoadModulesAsync();
    }

    public async Task LoadModulesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Modüller yükleniyor...";

            var currentFirmaId = 1; // Aktif seçili firma ID
            var result = await _moduleLicenseService.GetFirmaModulesAsync(currentFirmaId);

            Modules.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var mod in result.Data)
                {
                    Modules.Add(new ModuleItemViewModel
                    {
                        ModuleType = mod.ModuleType,
                        ModuleName = mod.ModuleName,
                        Description = mod.Description,
                        IsActive = mod.IsActive,
                        IconKey = mod.IconKey
                    });
                }
                StatusMessage = $"{Modules.Count} modül yüklendi.";
            }
            else
            {
                StatusMessage = result.Message ?? "Modüller alınamadı.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleModuleAsync(ModuleItemViewModel? item)
    {
        if (item == null) return;

        item.IsActive = !item.IsActive;
        var currentFirmaId = 1;
        var result = await _moduleLicenseService.SetModuleStatusAsync(currentFirmaId, item.ModuleType, item.IsActive);
        StatusMessage = result.Message ?? (item.IsActive ? "Modül aktifleştirildi." : "Modül pasifleştirildi.");
    }

    private async Task SaveAllAsync()
    {
        try
        {
            IsLoading = true;
            var currentFirmaId = 1;
            var activeModules = Modules.Where(m => m.IsActive).Select(m => m.ModuleType);
            var result = await _moduleLicenseService.UpdateActiveModulesAsync(currentFirmaId, activeModules);
            StatusMessage = result.Message ?? "Tüm modül tercihleri başarıyla kaydedildi.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class ModuleItemViewModel : ObservableObject
{
    private bool _isActive;

    public ModuleType ModuleType { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;

    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }
}
