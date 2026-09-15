using System.ComponentModel;

namespace MuhasibPro.Business.Contracts.UIServices
{
    /// <summary>
    /// StatusBar üst bilgileri yönetimi (kullanıcı + sistem veritabanı + aktif firma/dönem bağlamı).
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

        /// <summary>Aktif firma kısa unvanı (bağlam; boşsa gösterilmez).</summary>
        string FirmaAdi { get; set; }

        /// <summary>Aktif mali dönem etiketi (ör. "2026 Dönemi"; boşsa gösterilmez).</summary>
        string MaliDonemAdi { get; set; }

        /// <summary>Tenant (seçili dönem) veritabanı bağlantı durumu.</summary>
        bool IsTenantDatabaseConnection { get; set; }

        /// <summary>Tenant bağlantı mesajı (tooltip).</summary>
        string TenantDatabaseMessage { get; set; }
        #endregion

        #region Methods
        /// <summary>Sistem veritabanı durumunu ayarla (gerçek durumdan beslenir).</summary>
        void SetSistemDatabaseStatus(bool isConnected, string message = null);

        /// <summary>Aktif firma/dönem/tenant bağlamını yüklü tenant'tan yeniden oku ve yayınla
        /// (workspace girişinde çağrılır).</summary>
        void RefreshAktifBaglam();
        #endregion
    }
}
