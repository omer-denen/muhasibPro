using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Enum.DatabaseEnum;
using MuhasibPro.Extensions;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.ViewModels.Sistem.MaliDonemler;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public sealed partial class MaliDonemlerListControl : UserControl
{
    private bool _isLoaded;

    public MaliDonemlerListControl()
    {
        InitializeComponent();
        Loaded += (s, e) => _isLoaded = true;
        Unloaded += (s, e) => _isLoaded = false;
    }

    private async void OnYeniDonemClick(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm == null)
        {
            GetNotification()?.Show("Hata", "Sayfa bağlamı bulunamadı.", NotificationType.Warning);
            return;
        }
        var firma = vm.SelectedFirma;
        if (firma == null)
        {
            GetNotification()?.Show("Firma Seçilmedi", "Lütfen önce soldan bir şirket seçin.", NotificationType.Warning);
            return;
        }

        var dialog = new Dialogs.YeniDonemDialog
        {
            FirmaId = firma.Id,
            FirmaKodu = firma.FirmaKodu,
            FirmaUnvan = firma.KisaUnvani
        };
        var result = await DialogHelper.ShowCenteredAsync(dialog);
        if (result != ContentDialogResult.Primary && !dialog.EnterOnay)
            return;

        await Task.Delay(120);
        var pipeline = new SagaPipelineDialog
        {
            FirmaId = firma.Id,
            MaliYil = dialog.DonemYili,
            Aciklama = dialog.Aciklama
        };
        await DialogHelper.ShowCenteredAsync(pipeline);

        if (pipeline.SagaCompleted)
        {
            await vm.MaliDonemList.RefreshAsync();
        }
    }

    private async void OnBackupClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: MaliDonemModel model } || string.IsNullOrEmpty(model.DatabaseName))
            return;

        var notification = GetNotification();
        try
        {
            var service = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseOperationService>();
            var response = await service.CreateBackupAsync(model.DatabaseName, DatabaseBackupType.Safety);
            if (response.Success && response.Data != null && response.Data.IsBackupComleted)
            {
                // Yedek başarılı: Global.db satırına vitrin meta verisini işle,
                // sonra listeyi DB'den tazele — kart "Son Yedek"/"Boyut" böyle dolar.
                model.TenantDetails ??= new TenantDetailsModel();
                model.TenantDetails.SonYedekTarihi = DateTime.Now;
                if (response.Data.BackupFileSizeBytes > 0)
                    model.TenantDetails.DosyaBoyutu = response.Data.BackupFileSizeBytes;

                var donemService = ServiceLocator.Current.GetService<IMaliDonemService>();
                var updated = await donemService.UpdateMaliDonemAsync(model);
                if (updated.Success)
                {
                    notification?.Show("Yedek Alındı",
                        $"{model.MaliYil} dönemi yedeklendi: '{response.Data.BackupFilePath}'",
                        NotificationType.Success);
                }
                else
                {
                    notification?.Show("Yedek Alındı",
                        $"{model.MaliYil} dönemi yedeklendi ancak dönem kaydı güncellenemedi: {updated.Message}",
                        NotificationType.Warning);
                }

                var vm = GetViewModel();
                if (vm != null)
                    await vm.MaliDonemList.RefreshAsync();
            }
            else
            {
                notification?.Show("Yedek Alınamadı", response.Message, NotificationType.Warning);
            }
        }
        catch (Exception ex)
        {
            notification?.Show("Yedek Hatası", ex.Message, NotificationType.Danger);
        }
    }

    private async void OnMaliDonemRadioChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { DataContext: MaliDonemModel clicked })
            return;

        var vm = GetViewModel();
        if (vm == null)
            return;

        // TwoWay IsChecked zaten clicked.Selected=true yaptı; ViewModel tek doğruluk için SelectedItem senkronla (diğerleri false)
        if (!ReferenceEquals(vm.MaliDonemList.SelectedItem, clicked))
            vm.MaliDonemList.SelectedItem = clicked;

        var selectedDonem = clicked;

        // Kapalı dönem seçildiğinde sadece animasyonlu geri bildirim
        if (selectedDonem.KapaliMi)
        {
            if (_isLoaded)
            {
                ShowClosedWarning($"{selectedDonem.MaliYil} kapalı dönem — giriş engellendi. Lütfen açık bir dönem seçin.");
                ShakeDevamEtButton();
            }
            return;
        }

        await vm.MaliDonemList.AnalyzeDbStatusAsync(selectedDonem);
        var notification = GetNotification();
        if (selectedDonem.DbDosyaYokMu)
        {
            // Kart hafif: detay işlemler yönetim penceresinde — kullanıcıyı oraya yönlendir.
            notification?.Show("Veritabanı Dosyası Bulunamadı", $"{selectedDonem.MaliYil} döneminin veritabanı dosyası diskte yok. Bu döneme giriş yapılamaz — firma kartındaki Mali Dönem İşlemleri penceresinden kurtarın ya da kaydı temizleyin.", NotificationType.Danger);
            return;
        }
        if (selectedDonem.DbKontrolGerekliMi)
            notification?.Show("Veritabanı Hatası", $"{selectedDonem.MaliYil} dönemi veritabanında bir hata tespit edildi: {selectedDonem.DbAnalizDetay}", NotificationType.Danger);
        else if (selectedDonem.DbGuncellemeGerekliMi)
            notification?.Show("Güncelleme Gerekli", $"{selectedDonem.MaliYil} dönemi veritabanı güncellenmeyi bekliyor.", NotificationType.Warning);
    }

    private void ShowClosedWarning(string text)
    {
        try { ClosedWarningBorder?.ShowAsync(text, 2600); } catch { }
    }

    private void ShakeDevamEtButton()
    {
        try
        {
            var storyboard = new Storyboard();
            var anim = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(anim, DevamEtTransform);
            Storyboard.SetTargetProperty(anim, "TranslateX");
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0 });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(60), Value = -8 });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(120), Value = 8 });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(180), Value = -6 });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(240), Value = 6 });
            anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(300), Value = 0 });
            storyboard.Children.Add(anim);
            storyboard.Begin();
        }
        catch { }
    }

    /// <summary>DataContext → IMaliDonemListHost çözümle (FirmaShell VM veya yönetim VM; fallback: visual parent).</summary>
    private IMaliDonemListHost GetViewModel()
    {
        var vm = DataContext as IMaliDonemListHost;
        if (vm == null)
        {
            var parent = this.FindVisualParent<Views.ShellViews.Shell.FirmaShellView>();
            var shellVm = parent?.ViewModel as IMaliDonemListHost;
            vm = shellVm;
        }
        return vm;
    }

    private static INotificationService GetNotification()
        => ServiceLocator.Current.GetService<INotificationService>();
}