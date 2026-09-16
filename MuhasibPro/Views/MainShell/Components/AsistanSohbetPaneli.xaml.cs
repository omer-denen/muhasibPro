using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Collections.Specialized;

namespace MuhasibPro.Views.MainShell.Components
{
    /// <summary>AI yardım asistanı alt paneli (Faz 6.92). DataContext = AsistanSohbetViewModel.</summary>
    public sealed partial class AsistanSohbetPaneli : UserControl
    {
        private AsistanSohbetViewModel? _aboneVm;

        public AsistanSohbetPaneli()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        /// <summary>Panel başlığındaki "gizle" düğmesi — host (MainShellView) çekmeceyi kapatır.</summary>
        public event System.EventHandler GizleIstek;

        private void OnGizleClick(object sender, RoutedEventArgs e)
            => GizleIstek?.Invoke(this, System.EventArgs.Empty);

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_aboneVm != null)
                _aboneVm.Mesajlar.CollectionChanged -= MesajlarDegisti;

            _aboneVm = args.NewValue as AsistanSohbetViewModel;
            if (_aboneVm != null)
            {
                _aboneVm.Mesajlar.CollectionChanged += MesajlarDegisti;
                AsagiKaydir();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_aboneVm != null)
            {
                _aboneVm.Mesajlar.CollectionChanged -= MesajlarDegisti;
                _aboneVm = null;
            }
            DataContextChanged -= OnDataContextChanged;
            Unloaded -= OnUnloaded;
        }

        private void MesajlarDegisti(object? sender, NotifyCollectionChangedEventArgs e) => AsagiKaydir();

        private void AsagiKaydir()
        {
            if (SohbetKaydirici != null)
                SohbetKaydirici.ChangeView(null, SohbetKaydirici.ScrollableHeight, null);
        }

        private void OnSoruKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter
                && DataContext is AsistanSohbetViewModel vm
                && vm.GonderCommand.CanExecute(null))
            {
                vm.GonderCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
