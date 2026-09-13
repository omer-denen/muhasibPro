using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService;

namespace MuhasibPro.Views.SistemDbYonetim.Components.Dialogs;

/// <summary>
/// Faz 6.78 Adım 3: Sistem.db geri-yükleme hüküm dialogu (tek kapı).
/// Hükmü gösterir + (RequireCode ise) tek-seferlik kod doğrular; restore'u bu dialog yapmaz —
/// onay bayrağı döner, çağıran (VM) gerçek geri yüklemeyi yürütür.
/// </summary>
public sealed partial class SistemRestoreVerifyDialog : ContentDialog
{
    private SistemRestoreAnalizSonuc? _sonuc;
    private string _generatedCode = string.Empty;

    public bool GeriYukleOnaylandi { get; private set; }

    public SistemRestoreVerifyDialog()
    {
        InitializeComponent();
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    public void SetSonuc(SistemRestoreAnalizSonuc sonuc)
    {
        _sonuc = sonuc;

        BackupFileText.Text = string.IsNullOrWhiteSpace(sonuc.YedekDosyaAdi) ? "—" : sonuc.YedekDosyaAdi;
        BackupMetaText.Text = $"Boyut: {FormatSize(sonuc.Dosya.DosyaBoyutu)} • Tarih: {sonuc.Dosya.YedekTarihi:dd.MM.yyyy HH:mm} • Dosyada sürüm: {sonuc.Dosya.GocGecmisiSurumu ?? "-"}";

        if (sonuc.Fark.KayipVarMi)
        {
            FarkCard.Visibility = Visibility.Visible;
            FarkText.Text = sonuc.FarkDetayi;
        }

        ApplyVerdict(sonuc.Hukum);
    }

    private void ApplyVerdict(RestoreVerdict v)
    {
        VerdictTitle.Text = v.Baslik;
        VerdictDesc.Text = v.Aciklama;

        var res = Application.Current.Resources;
        switch (v.Kind)
        {
            case RestoreVerdictKind.Allow:
                Paint(res, "SystemFillColorSuccessBackgroundBrush", "SystemFillColorSuccessBrush");
                VerdictIcon.Glyph = "\uE73E";
                IsPrimaryButtonEnabled = true;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
            case RestoreVerdictKind.Warning:
                Paint(res, "SystemFillColorCautionBackgroundBrush", "SystemFillColorCautionBrush");
                VerdictIcon.Glyph = "\uE7BA";
                IsPrimaryButtonEnabled = true;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
            case RestoreVerdictKind.RequireCode:
                Paint(res, "SystemFillColorCriticalBackgroundBrush", "SystemFillColorCriticalBrush");
                VerdictIcon.Glyph = "\uE783";
                IsPrimaryButtonEnabled = true;
                GenerateCode();
                CodePanel.Visibility = Visibility.Visible;
                break;
            case RestoreVerdictKind.Block:
                VerdictBorder.Background = (Brush)res["CardBackgroundFillColorSecondaryBrush"];
                VerdictBorder.BorderBrush = (Brush)res["CardStrokeColorDefaultBrush"];
                VerdictTitle.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                VerdictDesc.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                VerdictIcon.Glyph = "\uE711";
                VerdictIcon.Foreground = (Brush)res["TextFillColorSecondaryBrush"];
                IsPrimaryButtonEnabled = false;
                CodePanel.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void Paint(ResourceDictionary res, string backgroundKey, string accentKey)
    {
        VerdictBorder.Background = (Brush)res[backgroundKey];
        VerdictBorder.BorderBrush = (Brush)res[accentKey];
        VerdictTitle.Foreground = (Brush)res[accentKey];
        VerdictDesc.Foreground = (Brush)res[accentKey];
        VerdictIcon.Foreground = (Brush)res[accentKey];
    }

    private void GenerateCode()
    {
        _generatedCode = new Random().Next(100000, 999999).ToString();
        GeneratedCodeText.Text = _generatedCode;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_sonuc == null)
        {
            args.Cancel = true;
            return;
        }
        if (_sonuc.Hukum.Engelli)
        {
            args.Cancel = true;
            return;
        }
        if (_sonuc.Hukum.KodGerekli)
        {
            var input = CodeInputBox.Text?.Trim() ?? string.Empty;
            if (input != _generatedCode)
            {
                ErrorText.Text = "Onay kodu hatalı. Lütfen 6 haneli kodu doğru girin.";
                ErrorBorder.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }
        }

        GeriYukleOnaylandi = true;
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
        return $"{len:0.##} {sizes[order]}";
    }
}
