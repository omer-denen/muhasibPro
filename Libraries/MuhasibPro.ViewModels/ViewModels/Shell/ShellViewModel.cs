using MuhasibPro.Business.Contracts.DatabaseServices.SistemDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class ShellArgs
    {
        public Type ViewModel { get; set; }
        public object Parameter { get; set; }
        public HesapModel UserInfo { get; set; }
    }
    public class ShellViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ISistemDatabaseService _sistemDatabaseService;

        public ShellViewModel(
            IAuthenticationService authenticationService,
            ISistemDatabaseService sistemDatabaseService,
            ICommonServices commonServices) : base(commonServices)
        {
            _authenticationService = authenticationService;
            _sistemDatabaseService = sistemDatabaseService;
            UpdateLockedStatus();
            _authenticationService.StateChanged += OnAuthenticationStateChanged;
        }

        private void OnAuthenticationStateChanged()
        {
            ContextService.RunAsync(() =>
            {
                UpdateLockedStatus();

                if (!_authenticationService.IsAuthenticated)
                {
                    UserInfo = null;
                    StatusBarService.UserName = string.Empty;
                }
                else
                {
                    UserInfo = _authenticationService.CurrentAccount;
                    UserInfoyuStatusBaraYaz();
                }
            });
        }

        private bool _isLocked = false;

        public bool IsLocked { get => _isLocked; set => Set(ref _isLocked, value); }

        private bool _isEnabled = true;

        public bool IsEnabled { get => _isEnabled; set => Set(ref _isEnabled, value); }

        public HesapModel UserInfo { get; protected set; }


        public ShellArgs ViewModelArgs { get; protected set; }

        public virtual Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            if (ViewModelArgs != null)
            {
                UserInfo = ViewModelArgs.UserInfo;
                UserInfoyuStatusBaraYaz();
                UpdateLockedStatus();
            }

            if (ViewModelArgs?.ViewModel != null)
                NavigationService.Navigate(ViewModelArgs.ViewModel, ViewModelArgs.Parameter);

            // Navigasyonu geciktirmeden durumu arka planda besle
            _ = SistemVeritabaniDurumunuYazAsync();
            return Task.CompletedTask;
        }

        public virtual void Unload()
        {
            if (_authenticationService != null)
            {
                _authenticationService.StateChanged -= OnAuthenticationStateChanged;
            }
        }
        public void Logout()
        {
            _authenticationService.Logout();
            UserInfo = null;
        }
        public virtual void Subscribe()
        {
            MessageService.Subscribe<IAuthenticationService, bool>(this, OnLoginMessage);
            MessageService.Subscribe<ViewModelBase, string>(this, OnMessage);
        }
        private void UpdateLockedStatus()
        {
            IsLocked = !_authenticationService.IsAuthenticated;
        }
        public virtual void Unsubscribe() { MessageService.Unsubscribe(this); }

        protected void UserInfoyuStatusBaraYaz()
        {
            if (UserInfo?.KullaniciModel != null)
                StatusBarService.UserName = UserInfo.KullaniciModel.AdiSoyadi;
        }

        /// <summary>Sistem veritabanı göstergesini gerçek durumdan besler (Kural 7).</summary>
        protected async Task SistemVeritabaniDurumunuYazAsync()
        {
            try
            {
                var resp = await _sistemDatabaseService.GetSistemDatabaseStateAsync();
                var state = resp?.Data;
                bool bagli = state != null && state.IsDatabaseExists && state.CanConnect && !state.HasError && state.DatabaseValid;
                var mesaj = state?.Message ?? resp?.Message
                    ?? (bagli ? "Sistem veritabanı bağlı" : "Sistem veritabanı bağlı değil");
                StatusBarService.SetSistemDatabaseStatus(bagli, mesaj);
            }
            catch
            {
                StatusBarService.SetSistemDatabaseStatus(false, "Sistem veritabanı durumu alınamadı");
            }
        }

        private async void OnLoginMessage(IAuthenticationService loginService, string message, bool isAuthenticated)
        {
            if (message == "AuthenticationChanged")
            {
                await ContextService.RunAsync(() =>
                {
                    UpdateLockedStatus();

                    if (!isAuthenticated)
                    {
                        UserInfo = null;
                        StatusBarService.UserName = string.Empty;
                    }
                    else if (UserInfo == null && _authenticationService.CurrentAccount != null)
                    {
                        UserInfo = _authenticationService.CurrentAccount;
                        UserInfoyuStatusBaraYaz();
                    }
                });
            }
        }

        private async void OnMessage(ViewModelBase viewModel, string message, string action)
        {
            switch (message)
            {
                case "EnableThisView":
                case "DisableThisView":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsEnabled = message == "EnableThisView";
                    }
                    break;

                case "EnableOtherViews":
                case "DisableOtherViews":
                    if (viewModel.ContextService.ContextId != ContextService.ContextId)
                    {
                        await ContextService.RunAsync(() => IsEnabled = message == "EnableOtherViews");
                    }
                    break;

                case "EnableAllViews":
                case "DisableAllViews":
                    await ContextService.RunAsync(() => IsEnabled = message == "EnableAllViews");
                    break;
            }
        }
    }
}
