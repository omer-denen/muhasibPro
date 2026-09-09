using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Views.ShellViews.Shell.Components;

namespace MuhasibPro.Converters;

public sealed class SagaStepStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var status = value is SagaStepStatus s ? s : SagaStepStatus.Pending;
        var kind = parameter as string ?? "Background";

        var res = Microsoft.UI.Xaml.Application.Current?.Resources;
        string key = (status, kind) switch
        {
            (SagaStepStatus.Pending, "Background") => "MuhasibInventPaleBrush",
            (SagaStepStatus.Pending, "Border") => "MuhasibSoftBorderBrush",
            (SagaStepStatus.Pending, "Foreground") => "MuhasibSageTertiaryBrush",
            (SagaStepStatus.Pending, "Icon") => "MuhasibSageTertiaryBrush",
            (SagaStepStatus.InProgress, "Background") => "MuhasibPetrolTintBrush",
            (SagaStepStatus.InProgress, "Border") => "MuhasibPetrolBorderBrush",
            (SagaStepStatus.InProgress, "Foreground") => "MuhasibTextPrimaryBrush",
            (SagaStepStatus.InProgress, "Icon") => "MuhasibPetrolBrush",
            (SagaStepStatus.Completed, "Background") => "MuhasibSuccessBgBrush",
            (SagaStepStatus.Completed, "Border") => "MuhasibSuccessBrush",
            (SagaStepStatus.Completed, "Foreground") => "MuhasibTextPrimaryBrush",
            (SagaStepStatus.Completed, "Icon") => "MuhasibSuccessBrush",
            (SagaStepStatus.Failed, "Background") => "MuhasibDangerBgBrush",
            (SagaStepStatus.Failed, "Border") => "MuhasibDangerBrush",
            (SagaStepStatus.Failed, "Foreground") => "MuhasibTextPrimaryBrush",
            (SagaStepStatus.Failed, "Icon") => "MuhasibDangerBrush",
            _ => "MuhasibInventPaleBrush"
        };

        if (res != null && res.TryGetValue(key, out var brush) && brush is Brush b) return b;
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
