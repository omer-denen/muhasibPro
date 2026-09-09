namespace MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel
{
    public class AnalysisProgress
    {
        public string Message { get; set; }
        public ProgressType Type { get; set; }
        public double Percentage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ProgressType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class AnalysisOptions
    {
        public static AnalysisOptions Default => new()
        {
            CheckIntegrity = true,
            CheckMigrations = true,
            BatchSize = 5,
            DelayBetweenBatches = TimeSpan.FromMilliseconds(50)
        };

        public bool CheckIntegrity { get; set; }
        public bool CheckMigrations { get; set; }
        public int BatchSize { get; set; }
        public TimeSpan DelayBetweenBatches { get; set; }
    }
}
