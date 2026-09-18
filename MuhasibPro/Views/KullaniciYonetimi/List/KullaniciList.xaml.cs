// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.



using MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi;

namespace MuhasibPro.Views.KullaniciYonetimi.List
{
    public sealed partial class KullaniciList : UserControl
    {
        public KullaniciList()
        {
            InitializeComponent();
        }
        private void KullaniciList_Loaded(object sender, RoutedEventArgs e)
        {
            if (pageTableView != null && dataList.ConfigControl != null)
            {
                dataList.ConfigControl.AttachTableView(pageTableView);
            }
        }
        #region ViewModel
        public KullaniciListViewModel ViewModel
        {
            get { return (KullaniciListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(KullaniciListViewModel),
            typeof(KullaniciList),
            new PropertyMetadata(null));
        #endregion
    }
}
