using System.ComponentModel;

namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>
    /// StatusBar üst bilgileri yönetimi (kullanıcı + sistem veritabanı durumu).
    /// Mesaj işlemleri için <see cref="IStatusMessageService"/> kullanın.
    /// </summary>
    public interface IStatusBarService : INotifyPropertyChanged
    {
        #region Properties
        /// <summary>Kullanıcı görünen adı (StatusBar'da gösterilir).</summary>
        string UserName { get; set; }

        /// <summary>Veritabanı bağlantı mesajı.</summary>
        string DatabaseConnectionMessage { get; set; }

        /// <summary>Sistem veritabanı bağlantı durumu (true: bağlı, false: bağlı değil).</summary>
        bool IsSistemDatabaseConnection { get; set; }
        #endregion

        #region Methods
        /// <summary>Sistem veritabanı durumunu ayarla (gerçek durumdan beslenir).</summary>
        void SetSistemDatabaseStatus(bool isConnected, string message = null);
        #endregion
    }
}
