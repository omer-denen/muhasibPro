using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices;

namespace MuhasibPro.Services.UIService
{
    public class StatusBarService : INotifyPropertyChanged, IStatusBarService, IDisposable
    {
        private readonly IFirmaWithMaliDonemSelectedService _selection;
        private string _userName;
        private string _databaseConnectionMessage;
        private bool _isSistemDatabaseConnection;
        private string _firmaAdi = string.Empty;
        private string _maliDonemAdi = string.Empty;
        private bool _isTenantDatabaseConnection;
        private string _tenantDatabaseMessage = string.Empty;

        public StatusBarService(IFirmaWithMaliDonemSelectedService selection)
        {
            _selection = selection;
            if (_selection != null)
                _selection.StateChanged += OnSelectionStateChanged;
            AktifBaglamiGuncelle();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Public Properties
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName == value) return;
                _userName = value;
                NotifyPropertyChanged(nameof(UserName));
            }
        }

        public string DatabaseConnectionMessage
        {
            get => _databaseConnectionMessage;
            set
            {
                if (_databaseConnectionMessage == value) return;
                _databaseConnectionMessage = value;
                NotifyPropertyChanged(nameof(DatabaseConnectionMessage));
            }
        }

        public bool IsSistemDatabaseConnection
        {
            get => _isSistemDatabaseConnection;
            set
            {
                if (_isSistemDatabaseConnection == value) return;
                _isSistemDatabaseConnection = value;
                NotifyPropertyChanged(nameof(IsSistemDatabaseConnection));
            }
        }

        public string FirmaAdi
        {
            get => _firmaAdi;
            set
            {
                if (_firmaAdi == value) return;
                _firmaAdi = value;
                NotifyPropertyChanged(nameof(FirmaAdi));
            }
        }

        public string MaliDonemAdi
        {
            get => _maliDonemAdi;
            set
            {
                if (_maliDonemAdi == value) return;
                _maliDonemAdi = value;
                NotifyPropertyChanged(nameof(MaliDonemAdi));
            }
        }

        public bool IsTenantDatabaseConnection
        {
            get => _isTenantDatabaseConnection;
            set
            {
                if (_isTenantDatabaseConnection == value) return;
                _isTenantDatabaseConnection = value;
                NotifyPropertyChanged(nameof(IsTenantDatabaseConnection));
            }
        }

        public string TenantDatabaseMessage
        {
            get => _tenantDatabaseMessage;
            set
            {
                if (_tenantDatabaseMessage == value) return;
                _tenantDatabaseMessage = value;
                NotifyPropertyChanged(nameof(TenantDatabaseMessage));
            }
        }
        #endregion

        public void SetSistemDatabaseStatus(bool isConnected, string message = null)
        {
            ExecuteOnUIThread(() =>
            {
                IsSistemDatabaseConnection = isConnected;
                if (!string.IsNullOrEmpty(message))
                    DatabaseConnectionMessage = message;
            });
        }

        private void OnSelectionStateChanged() => ExecuteOnUIThread(AktifBaglamiGuncelle);

        /// <summary>Aktif bağlamı (firma/dönem/tenant) gerçek durumdan yeniden okur ve yayınlar.</summary>
        public void RefreshAktifBaglam() => ExecuteOnUIThread(AktifBaglamiGuncelle);

        /// <summary>Aktif firma/dönem yalnız GERÇEK yüklü tenant (ConnectedTenantDb.IsLoaded) varken
        /// gösterilir; ham seçim tek başına yetmez (çoklu firma/dönemde eski seçim görünmesin — Kural 7).</summary>
        private void AktifBaglamiGuncelle()
        {
            var tenant = _selection?.ConnectedTenantDb;
            bool yuklu = tenant?.IsLoaded == true;

            IsTenantDatabaseConnection = yuklu;
            if (yuklu)
            {
                FirmaAdi = _selection?.SelectedFirma?.KisaUnvani ?? string.Empty;
                MaliDonemAdi = _selection?.SelectedMaliDonem is { MaliYil: > 0 } donem
                    ? $"{donem.MaliYil} Dönemi"
                    : string.Empty;
                TenantDatabaseMessage = string.IsNullOrWhiteSpace(tenant?.Message)
                    ? "Tenant veritabanı bağlı"
                    : tenant.Message;
            }
            else
            {
                FirmaAdi = string.Empty;
                MaliDonemAdi = string.Empty;
                TenantDatabaseMessage = "Tenant veritabanı bağlı değil";
            }
        }

        public void Dispose()
        {
            if (_selection != null)
                _selection.StateChanged -= OnSelectionStateChanged;
        }

        private static void ExecuteOnUIThread(Action action)
        {
            var queue = App._dispatcherQueue;
            if (queue == null || queue.HasThreadAccess)
                action();
            else
                queue.TryEnqueue(() => action());
        }

        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
