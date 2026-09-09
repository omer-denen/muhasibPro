using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

public sealed partial class TransferDialog : ContentDialog
{
    public TransferDialog()
    {
        InitializeComponent();
    }

    public string CurrentKurulumId
    {
        get => CurrentKurulumText.Text;
        set => CurrentKurulumText.Text = value ?? "-";
    }

    public string CurrentMachineId
    {
        get => CurrentMachineText.Text;
        set => CurrentMachineText.Text = value ?? "-";
    }

    public void SetMismatches(IEnumerable<string> items)
    {
        MismatchList.ItemsSource = items;
    }
}
