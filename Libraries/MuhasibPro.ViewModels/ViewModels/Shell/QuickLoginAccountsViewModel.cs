using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

public class RememberedAccount : ObservableObject
{
    private string _username = string.Empty;
    public string Username { get => _username; set { if (Set(ref _username, value)) NotifyPropertyChanged(nameof(Initial)); } }

    private string _displayName = string.Empty;
    public string DisplayName { get => _displayName; set { if (Set(ref _displayName, value)) NotifyPropertyChanged(nameof(Initial)); } }

    private string _roleName = string.Empty;
    public string RoleName { get => _roleName; set => Set(ref _roleName, value); }

    public string Initial => string.IsNullOrWhiteSpace(DisplayName) ? (string.IsNullOrWhiteSpace(Username) ? "?" : Username.Substring(0, 1).ToUpperInvariant()) : DisplayName.Substring(0, 1).ToUpperInvariant();
}

public class QuickLoginAccountsViewModel : ViewModelBase
{
    private const string RememberedAccountsKey = "LoginRememberedAccounts";
    private const string RememberMeKey = "LoginIsRememberMe";
    private readonly ILocalSettingsService _localSettingsService;

    public QuickLoginAccountsViewModel(ICommonServices commonServices, ILocalSettingsService localSettingsService) : base(commonServices)
    {
        _localSettingsService = localSettingsService;
        _ = LoadRememberedAccountsAsync();
    }

    public ObservableCollection<RememberedAccount> RememberedAccounts { get; } = new ObservableCollection<RememberedAccount>();

    private bool _isRememberMe;
    public bool IsRememberMe
    {
        get => _isRememberMe;
        set
        {
            if(Set(ref _isRememberMe, value))
            {
                _ = _localSettingsService.SaveSettingAsync(RememberMeKey, value);
            }
        }
    }

    // Beni hatırla: seçiliyken anlık ekle, kaldırıldığında sil — sadece kullanıcı adı, şifre asla kaydedilmez
    public void SyncRememberedForUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return;
        var key = username.Trim();
        var exists = RememberedAccounts.Any(r => string.Equals(r.Username, key, StringComparison.OrdinalIgnoreCase));
        if (IsRememberMe && !exists)
        {
            var isAdmin = string.Equals(key, "korkutomer", StringComparison.OrdinalIgnoreCase);
            RememberedAccounts.Add(new RememberedAccount
            {
                Username = key,
                DisplayName = isAdmin ? "Sistem Admin" : key,
                RoleName = isAdmin ? "Sistem Admin" : "Kullanıcı"
            });
            _ = SaveRememberedAccountsAsync();
        }
        else if (!IsRememberMe && exists)
        {
            var existing = RememberedAccounts.FirstOrDefault(r => string.Equals(r.Username, key, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                RememberedAccounts.Remove(existing);
                _ = SaveRememberedAccountsAsync();
            }
        }
    }

    public void AddOrUpdateForSuccessfulLogin(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return;
        if (!IsRememberMe) return;
        var key = username.Trim();
        var exists = RememberedAccounts.Any(r => string.Equals(r.Username, key, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            var isAdmin = string.Equals(key, "korkutomer", StringComparison.OrdinalIgnoreCase);
            RememberedAccounts.Add(new RememberedAccount
            {
                Username = key,
                DisplayName = isAdmin ? "Sistem Admin" : key,
                RoleName = isAdmin ? "Sistem Admin" : "Kullanıcı"
            });
            _ = SaveRememberedAccountsAsync();
        }
    }

    public ICommand SelectRememberedAccountCommand => new RelayCommand<RememberedAccount>(acc =>
    {
        if (acc == null) return;
        // Seçim MessageService ile LoginViewModel'e bildirilir
        MessageService.Send(this, "QuickLoginSelected", acc);
    });

    public ICommand RemoveRememberedAccountCommand => new RelayCommand<RememberedAccount>(acc =>
    {
        if (acc == null) return;
        RememberedAccounts.Remove(acc);
        _ = SaveRememberedAccountsAsync();
        MessageService.Send(this, "QuickLoginRemoved", acc);
    });

    private async Task LoadRememberedAccountsAsync()
    {
        try
        {
            var remember = await _localSettingsService.ReadSettingAsync<bool?>(RememberMeKey);
            _isRememberMe = remember ?? false;
            NotifyPropertyChanged(nameof(IsRememberMe));

            var saved = await _localSettingsService.ReadSettingAsync<List<RememberedAccount>>(RememberedAccountsKey);
            await ContextService.RunAsync(() =>
            {
                RememberedAccounts.Clear();
                if (!_isRememberMe)
                {
                    if (saved != null && saved.Count != 0)
                        _ = SaveRememberedAccountsAsync();
                    return;
                }
                if (saved != null && saved.Count > 0)
                {
                    // Kaydedilmiş tüm kullanıcılar (admin dahil) — varsayılanlar temizlendi, sadece hatırlananlar
                    foreach (var acc in saved.DistinctBy(a => a.Username.ToLowerInvariant()))
                    {
                        if (string.IsNullOrWhiteSpace(acc.Username)) continue;
                        var key = acc.Username.Trim();
                        var isAdmin = string.Equals(key, "korkutomer", StringComparison.OrdinalIgnoreCase);
                        RememberedAccounts.Add(new RememberedAccount
                        {
                            Username = key,
                            DisplayName = isAdmin ? "Sistem Admin" : (string.IsNullOrWhiteSpace(acc.DisplayName) ? key : acc.DisplayName),
                            RoleName = isAdmin ? "Sistem Admin" : (string.IsNullOrWhiteSpace(acc.RoleName) ? "Kullanıcı" : acc.RoleName)
                        });
                    }
                }
            });
        }
        catch { }
    }

    private async Task SaveRememberedAccountsAsync()
    {
        try
        {
            var list = RememberedAccounts.ToList();
            await _localSettingsService.SaveSettingAsync(RememberedAccountsKey, list);
        }
        catch { }
    }
}
