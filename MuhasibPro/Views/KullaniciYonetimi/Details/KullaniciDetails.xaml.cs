// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi.Details
{
    public sealed partial class KullaniciDetails : UserControl
    {
        public KullaniciDetails()
        {
            InitializeComponent();
        }
        #region ViewModel
        public KullaniciDetailsViewModel ViewModel
        {
            get { return (KullaniciDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(KullaniciDetailsViewModel), typeof(KullaniciDetails), new PropertyMetadata(null));
        #endregion
    }
}
