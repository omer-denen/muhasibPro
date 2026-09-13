using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace MuhasibPro.Views.Components;

/// <summary>Yardım maddesi: başlık + açıklama (Kural 13 ortak deseni).</summary>
public sealed class YardimMaddesi
{
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
}

/// <summary>
/// Ortak yardım dialogu (Kural 13): sayfa başlığındaki ? butonu bunu açar.
/// Kural 8 kaydı: CEKIRDEK-MODUL-PLAN.md "Ortak Controls Kaydı".
/// </summary>
public sealed partial class YardimDialog : ContentDialog
{
    public YardimDialog()
    {
        this.InitializeComponent();
    }

    public void IcerikAta(string baslik, IList<YardimMaddesi> maddeler)
    {
        BaslikMetni.Text = baslik;
        MaddeListesi.ItemsSource = maddeler;
    }
}
