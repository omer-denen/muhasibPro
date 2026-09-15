using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>Tablo değişikliğinin ekrana hazır hali (Expander başlık + kolon grupları).</summary>
    public class TenantTableDisplay
    {
        public TenantTableDisplay(TenantTableChange change)
        {
            Table = change.Table;
            TitleLine = change.IsCreated ? $"'{change.Table}' tablosu oluşturuldu" : $"'{change.Table}' tablosu güncellemesi";
            Added = new List<string>(change.AddedColumns);
            Updated = new List<string>(change.AlteredColumns);
            Removed = new List<string>(change.RemovedColumns);
        }

        public string Table { get; }
        public string TitleLine { get; }
        public List<string> Added { get; }
        public List<string> Updated { get; }
        public List<string> Removed { get; }
        public bool HasAdded => Added.Count > 0;
        public bool HasUpdated => Updated.Count > 0;
        public bool HasRemoved => Removed.Count > 0;
    }
}
