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
            (SagaStepStatus.Pending, "Background") => "CardBackgroundFillColorSecondaryBrush",
            (SagaStepStatus.Pending, "Border") => "CardStrokeColorDefaultBrush",
            (SagaStepStatus.Pending, "Foreground") => "TextFillColorTertiaryBrush",
            (SagaStepStatus.Pending, "Icon") => "TextFillColorTertiaryBrush",
            (SagaStepStatus.InProgress, "Background") => "MuhasibPetrolTintBrush",
            (SagaStepStatus.InProgress, "Border") => "MuhasibPetrolBorderBrush",
            (SagaStepStatus.InProgress, "Foreground") => "TextFillColorPrimaryBrush",
            (SagaStepStatus.InProgress, "Icon") => "MuhasibPetrolBrush",
            (SagaStepStatus.Completed, "Background") => "SystemFillColorSuccessBackgroundBrush",
            (SagaStepStatus.Completed, "Border") => "SystemFillColorSuccessBrush",
            (SagaStepStatus.Completed, "Foreground") => "TextFillColorPrimaryBrush",
            (SagaStepStatus.Completed, "Icon") => "SystemFillColorSuccessBrush",
            (SagaStepStatus.Failed, "Background") => "SystemFillColorCriticalBackgroundBrush",
            (SagaStepStatus.Failed, "Border") => "SystemFillColorCriticalBrush",
            (SagaStepStatus.Failed, "Foreground") => "TextFillColorPrimaryBrush",
            (SagaStepStatus.Failed, "Icon") => "SystemFillColorCriticalBrush",
            _ => "CardBackgroundFillColorSecondaryBrush"
        };

        if (res != null && res.TryGetValue(key, out var brush) && brush is Brush b) return b;
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
