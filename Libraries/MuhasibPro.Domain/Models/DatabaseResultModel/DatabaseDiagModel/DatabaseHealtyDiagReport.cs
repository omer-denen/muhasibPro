using MuhasibPro.Domain.Models.DatabaseResultModel;

namespace MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel
{
    public class DatabaseHealtyDiagReport : DatabaseAnalysisResult
    {
        public IProgress<AnalysisProgress> AnalysisProgress { get; set; }

        // Etiket ismi
        public override string OperationDisplayName => "Tanılama Test";
    }
}