using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.Shell.Tenant;

namespace MuhasibPro.Views.ShellViews.Shell
{
    /// <summary>Veritabanı güncelleme sayfası (bilgi + adımlar + doğrulama + geri alma).</summary>
    public sealed partial class TenantDatabaseUpdateView : Page
    {
        public TenantDatabaseUpdateView()
        {
            ViewModel = ServiceLocator.Current.GetService<TenantDatabaseUpdateViewModel>();
            InitializeComponent();
        }

        public TenantDatabaseUpdateViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var shellArgs = e.Parameter as ShellArgs;
            await ViewModel.LoadAsync(shellArgs?.Parameter as TenantDatabaseUpdateArgs);
        }
    }
}
