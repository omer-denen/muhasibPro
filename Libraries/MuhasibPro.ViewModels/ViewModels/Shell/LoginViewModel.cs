using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Domain.Common;
using MuhasibPro.Domain.Exceptions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

public class LoginViewModel : ViewModelBase
{
    public IAuthenticationService AuthenticationService { get; }

    public IFirmaService FirmaService { get; }

    private readonly QuickLoginAccountsViewModel _quickLoginVm;
    private readonly ISistemDatabaseService _sistemDatabaseService;
    private bool _quickLoginAbone;
    public QuickLoginAccountsViewModel QuickLogin => _quickLoginVm;

    public LoginViewModel(
        ICommonServices commonServices,
        IAuthenticationService authenticationService,
        IFirmaService firmaService,
        QuickLoginAccountsViewModel quickLoginVm,
        ISistemDatabaseService sistemDatabaseService) : base(commonServices)
    {
        AuthenticationService = authenticationService;
        FirmaService = firmaService;
        _quickLoginVm = quickLoginVm;
        _sistemDatabaseService = sistemDatabaseService;
        _isRememberMe = _quickLoginVm.IsRememberMe;
        _quickLoginVm.PropertyChanged += OnQuickLoginPropertyChanged;
    }

    // QuickLogin singleton kayıtlı "beni hatırla"yı asenkron yükler; bittiğinde checkbox senkronlanır (race fix)
    private void OnQuickLoginPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(QuickLoginAccountsViewModel.IsRememberMe)) return;
        _isRememberMe = _quickLoginVm.IsRememberMe;
        NotifyPropertyChanged(nameof(IsRememberMe));
    }


    private string _username = "korkutomer";
    private string _password = "Ok241341";
    private string _errorMessage = string.Empty;
    private bool _isRememberMe;

    public string Username
    {
        get => _username;
        set
        {
            if (Set(ref _username, value))
            {
                NotifyPropertyChanged(nameof(CanLogin));
                ErrorMessage = string.Empty;
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (Set(ref _password, value))
            {
                NotifyPropertyChanged(nameof(CanLogin));
                ErrorMessage = string.Empty;
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (Set(ref _errorMessage, value))
            {
                NotifyPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsNotBusy => !IsBusy;
    public bool CanLogin => DbIsReady && !IsBusy && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    public bool IsRememberMe
    {
        get => _isRememberMe;
        set
        {
            if(Set(ref _isRememberMe, value))
            {
                _quickLoginVm.IsRememberMe = value;
                _quickLoginVm.SyncRememberedForUser(Username);
            }
        }
    }

    public override bool IsBusy
    {
        get => base.IsBusy;
        set
        {
            if (base.IsBusy == value) return;
            base.IsBusy = value;
            NotifyPropertyChanged(nameof(IsNotBusy));
            NotifyPropertyChanged(nameof(CanLogin));
        }
    }

    private string _dbStatusText = "Kontrol ediliyor...";
    public string DbStatusText { get => _dbStatusText; set => Set(ref _dbStatusText, value); }
    private string _dbStatusDetail = "Sistem.db • SQLite";
    public string DbStatusDetail { get => _dbStatusDetail; set => Set(ref _dbStatusDetail, value); }
    private bool _dbIsReady;
    public bool DbIsReady { get => _dbIsReady; set { if(Set(ref _dbIsReady, value)) { NotifyPropertyChanged(nameof(DbStatusPillText)); NotifyPropertyChanged(nameof(DbStatusPillBrush)); NotifyPropertyChanged(nameof(CanLogin)); } } }
    public string DbStatusPillText => DbIsReady ? "Hazır" : "Kontrol";
    public string DbStatusPillBrush => DbIsReady ? "MuhasibSuccessBrush" : "MuhasibWarningBrush";

    public async Task LoadAsync(ShellArgs args)
    {
        ViewModelArgs = args;
        IsLoginWithPassword = true;
        IsBusy = false;
        ErrorMessage = string.Empty;
        SubscribeQuickLogin();
        await RefreshDbStatusAsync();
    }

    /// <summary>Tek seferlik abonelik — LoadAsync her çağrıda tekrar abone olmaz
    /// (MessageService aynı hedef+mesajda ikinci Add'de ArgumentException atar).</summary>
    private void SubscribeQuickLogin()
    {
        if (_quickLoginAbone)
            return;
        MessageService.Subscribe<QuickLoginAccountsViewModel, RememberedAccount>(this, (sender, message, acc) =>
        {
            if (message == "QuickLoginSelected" && acc != null)
            {
                Username = acc.Username;
                Password = acc.Username == "korkutomer" ? "Ok241341" : "123456";
            }
        });
        _quickLoginAbone = true;
    }

    public void Unsubscribe()
    {
        MessageService.Unsubscribe(this);
        _quickLoginAbone = false;
    }

    private async Task RefreshDbStatusAsync()
    {
        IsBusy = true;
        try
        {
            var resp = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
            var state = resp.Data;
            if (state == null)
            {
                DbIsReady = false;
                DbStatusText = "Sistem Veritabanı";
                DbStatusDetail = "Sistem.db • durum bilinmiyor";
                return;
            }
            // Success yok — modelden gelen verilerle karar (IsDatabaseExists/CanConnect/HasError/DatabaseValid)
            await ContextService.RunAsync(() =>
            {
                DbIsReady = state.IsDatabaseExists && state.CanConnect && !state.HasError && state.DatabaseValid;
                DbStatusText = DbIsReady ? "Sistem Veritabanı" : "Sistem Veritabanı — Kurulum Gerekli";
                var size = state.DatabaseFileSizeBytes > 0 ? $" • {state.DatabaseFileSizeBytes / 1024} KB" : "";
                var pending = state.PendingMigrations?.Count > 0 ? $" • {state.PendingMigrations.Count} bekleyen" : " • WAL";
                DbStatusDetail = $"Sistem.db • SQLite{size}{pending}";
            });
        }
        catch
        {
            DbIsReady = false;
            DbStatusText = "Sistem Veritabanı";
            DbStatusDetail = "Sistem.db • durum bilinmiyor";
        }
        finally { IsBusy = false; }
    }

    private ShellArgs ViewModelArgs { get; set; }

    private ICommand _loginWithPasswordCommand;
    public ICommand LoginWithPasswordCommand => _loginWithPasswordCommand ??= new AsyncRelayCommand(Login);


    private Result ValidateInput()
    {
        if(String.IsNullOrWhiteSpace(Username))
        {
            return Result.Error("Giriş Hatası", "Kullanıcı adı alanı boş geçilemez!");
        }
        if(String.IsNullOrWhiteSpace(Password))
        {
            return Result.Error("Giriş Hatası", "Şifre alanı boş geçilemez!");
        }
        return Result.Ok();
    }

    private bool _isLoginWithPassword = false;

    public bool IsLoginWithPassword
    {
        get { return _isLoginWithPassword; }
        set { Set(ref _isLoginWithPassword, value); }
    }

    public async Task Login()
    {
        ViewModelArgs.UserInfo = null;
        if(IsLoginWithPassword)
        {
            await LoginWithPassword();
        }
    }


    public async Task LoginWithPassword()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        var result = ValidateInput();
        if(result.IsOk)
        {
            try
            {
                await AuthenticationService.Login(Username, Password);
                if(AuthenticationService.IsAuthenticated)
                {
                    // Beni hatırla — başarılı girişte QuickLogin'e kaydet
                    _quickLoginVm.IsRememberMe = IsRememberMe;
                    _quickLoginVm.AddOrUpdateForSuccessfulLogin(Username);
                    ViewModelArgs.UserInfo = AuthenticationService.CurrentAccount;
                    await EnterApplication();
                    return;
                }
                result = Result.Error("Giriş Hatası", "Kullanıcı adı veya şifre hatalı!");
            } catch(UserNotFoundException)
            {
                result = Result.Error("Giriş Hatası", "Kullanıcı adı veya şifre hatalı!");
            } catch(InvalidPasswordException)
            {
                result = Result.Error("Giriş Hatası", "Kullanıcı adı veya şifre hatalı!");
            } finally
            {
                IsBusy = false;
            }
        }
        else
        {
            IsBusy = false;
        }
        ErrorMessage = result.Description ?? result.Message;
        if (!HasError)
            await DialogService.ShowAsync(result.Message, result.Description);
    }


    private async Task EnterApplication()
    {
        try
        {
            NavigationService.Navigate<FirmaShellViewModel>(ViewModelArgs);
            await LogService.SistemLogService
            .SistemLogInformationAsync("Login", "Sisteme giriş yapıldı", "Kullanıcı girişi", $"uygulamaya giriş yapıldı");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Bilinmeyen Hata", ex.Message);
        }
    }
}
