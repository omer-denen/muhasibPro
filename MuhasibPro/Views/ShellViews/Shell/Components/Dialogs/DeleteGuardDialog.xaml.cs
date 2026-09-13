using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.HostBuilders;

namespace MuhasibPro.Views.ShellViews.Shell.Components.Dialogs;

public sealed partial class DeleteGuardDialog : ContentDialog
{
    private readonly ITenantSQLiteDatabaseService _tenantService;

    public DeleteGuardDialog()
    {
        InitializeComponent();
        _tenantService = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseService>();
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private MaliDonemModel? _donem;
    public MaliDonemModel? Donem
    {
        get => _donem;
        set
        {
            _donem = value;
            if (_donem != null)
            {
                DbDosyaAdiText.Text = _donem.DatabaseName ?? string.Empty;
                DonemYiliText.Text = _donem.MaliYil.ToString();
                ConfirmCodeHintText.Text = _donem.MaliYil.ToString();
            }
        }
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        args.Cancel = true;

        if (_donem == null)
        {
            ShowError("Dönem bilgisi alınamadı.");
            deferral.Complete();
            return;
        }

        if (ConfirmCodeBox.Text.Trim() != _donem.MaliYil.ToString())
        {
            ShowError("Onay kodu hatalı. Lütfen dönemin yılını doğru yazınız.");
            deferral.Complete();
            return;
        }

        var yedekleriDeSil = YedekleriDeSilCheckBox.IsChecked != false;
        var request = new TenantDeletingRequest
        {
            MaliDonemId = _donem.Id,
            DatabaseName = _donem.DatabaseName,
            IsDeleteMaliDonem = true,
            IsDeleteDatabase = true,
            DeleteAllTenantBackup = yedekleriDeSil,
            IsCurrentTenantDeletingBeforeBackup = false
        };

        try
        {
            var response = await _tenantService.DeleteTenantDatabaseAsync(request);
            if (response.Success && response.Data.DeleteCompleted)
            {
                var bilgi = yedekleriDeSil
                    ? $"{_donem.MaliYil} dönemi ve veritabanı kaldırıldı."
                    : $"{_donem.MaliYil} dönemi kaldırıldı. Yedekler korundu (Silinen Dönem Yedekleri'nde).";
                ServiceLocator.Current.GetService<INotificationService>()?.Show("Mali Dönem Silindi", bilgi, NotificationType.Success);
                Hide();
            }
            else
            {
                ShowError(response.Message ?? "Silme işlemi başarısız oldu.");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }
}