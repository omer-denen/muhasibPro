namespace MuhasibPro.Business.Contracts.UIServices.CommonServices.Events
{
    /// <summary>Modül olay adları — mevcut ham string'lerin tek kaynağı (davranış değişmez, faz faz tipliye taşınır).</summary>
    public static class AppEvents
    {
        public const string SettingsChanged = "AppSettingsChanged";
        public const string StatusAnnounced = "StatusAnnounced";
    }

    public static class SystemDbEvents
    {
        public const string StateChanged = "SystemDbStateChanged";
        public const string BackupCompleted = "SystemBackupCompleted";
    }

    public static class EntityEvents
    {
        public const string ItemSelected = "ItemSelected";
        public const string NewItemSaved = "NewItemSaved";
        public const string ItemChanged = "ItemChanged";
        public const string ItemDeleted = "ItemDeleted";
        public const string ItemsDeleted = "ItemsDeleted";
        public const string ItemRangesDeleted = "ItemRangesDeleted";

        /// <summary>Firma kanadı — ItemChanged ile aynı hat (M3 seçim aynası semantiği).</summary>
        public const string FirmaChanged = ItemChanged;

        /// <summary>Dönem kanadı — ItemChanged ile aynı hat (M3 seçim aynası semantiği).</summary>
        public const string MaliDonemChanged = ItemChanged;
    }

    public static class TenantEvents
    {
        public const string Changed = "TenantChanged";
        public const string UpdateAvailable = "TenantUpdateAvailable";
        public const string Updated = "TenantUpdated";
        public const string BackupCompleted = "TenantBackupCompleted";
        public const string RestoreCompleted = "TenantRestoreCompleted";
        public const string TransferDetected = "TenantTransferDetected";
    }

    public static class IdentityEvents
    {
        public const string AuthenticationChanged = "AuthenticationChanged";
        public const string LicenseChanged = "LicenseChanged";
        public const string PermissionChanged = "PermissionChanged";
        public const string QuickLoginSelected = "QuickLoginSelected";
        public const string QuickLoginRemoved = "QuickLoginRemoved";
        public const string LogAdded = "LogAdded";
    }
}
