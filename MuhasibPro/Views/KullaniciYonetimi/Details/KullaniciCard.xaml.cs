// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi.Details
{
    public sealed partial class KullaniciCard : UserControl
    {
        public KullaniciCard()
        {
            InitializeComponent();
        }
        #region ViewModel
        public KullaniciDetailsViewModel ViewModel
        {
            get { return (KullaniciDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(KullaniciDetailsViewModel), typeof(KullaniciCard), new PropertyMetadata(null));
        #endregion

        #region Item
        public KullaniciModel Item
        {
            get { return (KullaniciModel)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(KullaniciModel), typeof(KullaniciCard), new PropertyMetadata(null));
        #endregion
    }
}
