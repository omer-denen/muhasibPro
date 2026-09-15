using MuhasibPro.Business.Contracts.UIServices;

namespace MuhasibPro.Services.UIService
{
    public class StatusBarService : INotifyPropertyChanged, IStatusBarService
    {
        private string _userName;
        private string _databaseConnectionMessage;
        private bool _isSistemDatabaseConnection;

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
