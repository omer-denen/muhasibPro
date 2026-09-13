using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Domain.Utilities.Responses;
using MuhasibPro.HostBuilders;
using System.Collections.ObjectModel;

namespace MuhasibPro.Views.ShellViews.Shell.Components;

public sealed partial class SagaPipelineDialog : ContentDialog
{
    private readonly ITenantSQLiteDatabaseService _tenantService;
    private bool _running;
    private int _activeIndex = -1;

    public ObservableCollection<SagaStepItem> Steps { get; } = new();

    /// <summary>Saga başarıyla tamamlandı mı — dış çağırıcı (MaliDonemlerListControl) RefreshAsync için kontrol eder.</summary>
    public bool SagaCompleted { get; private set; }

    public SagaPipelineDialog()
    {
        InitializeComponent();
        _tenantService = ServiceLocator.Current.GetService<ITenantSQLiteDatabaseService>();
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    public long FirmaId { get; set; }
    public int MaliYil { get; set; }
    public string Aciklama { get; set; } = string.Empty;

    private static string Now() => DateTime.Now.ToString("HH:mm:ss");

    private void BuildSteps()
    {
        Steps.Clear();
        Steps.Add(new SagaStepItem(1, "Guard Validation", "Firma ve mali yıl geçerlilik kontrolleri"));
        Steps.Add(new SagaStepItem(2, "DDL & WAL Init", "SQLite dosyası tahsisi ve WAL modu başlatma"));
        Steps.Add(new SagaStepItem(3, "69 Migration & Seed", "EF Core 69 tablo şeması + tekdüzen hesap planı"));
        Steps.Add(new SagaStepItem(4, "Global.db Sicil", "Global veritabanına dönem kaydı ve commit"));
        Steps.Add(new SagaStepItem(5, "SHA-256 Commit", "Bütünlük doğrulaması ve atomik tamamlama"));
        _activeIndex = -1;
        SetStatus("Saga hattı başlatıldı — atomik olarak yürütülüyor...", failed: false);
        StatusIcon.Glyph = "\uE823";
    }

    private void SetStatus(string text, bool failed)
    {
        StatusText.Text = text;
        var res = Microsoft.UI.Xaml.Application.Current?.Resources;
        var key = failed ? "SystemFillColorCriticalBrush" : "MuhasibPetrolBrush";
        if (res != null && res.TryGetValue(key, out var brush) && brush is Brush b)
        {
            StatusText.Foreground = b;
            StatusIcon.Foreground = b;
        }
    }

    private void SetStepStatus(int index, SagaStepStatus status)
    {
        if (index < 0 || index >= Steps.Count) return;
        var step = Steps[index];
        step.Status = status;
        step.Timestamp = Now();
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        args.Cancel = true;

        if (_running)
        {
            deferral.Complete();
            return;
        }
        if (FirmaId <= 0)
        {
            SetStatus("Firma seçili değil — işlem başlatılamadı.", failed: true);
            deferral.Complete();
            return;
        }

        _running = true;
        IsPrimaryButtonEnabled = false;
        IsSecondaryButtonEnabled = false;
        BuildSteps();
        SetStepStatus(0, SagaStepStatus.InProgress);
        _activeIndex = 0;

        var auth = ServiceLocator.Current.GetService<IAuthenticationService>();
        var request = new TenantCreationRequest
        {
            FirmaId = FirmaId,
            MaliYil = MaliYil,
            AutoCreateDatabase = true,
            DatabaseName = string.Empty,
            OlusturanRol = auth != null && auth.IsAuthenticated ? MuhasibPro.Domain.Entities.SistemEntity.KullaniciRolTip.Yönetici : null
        };

        ApiDataResponse<TenantCreationResult>? response = null;
        string? errorMessage = null;
        try
        {
            var operation = _tenantService.CreateNewTenantDatabaseAsync(request);
            await AdvanceWhileRunningAsync(operation);
            response = await operation;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            deferral.Complete();
        }

        if (response != null && response.Success && response.Data != null && response.Data.DatabaseCreated)
        {
            CompleteAllSteps();
            SetStatus("Saga başarıyla tamamlandı — mali dönem hazır.", failed: false);
            StatusIcon.Glyph = "\uE73E";
            SagaCompleted = true;
            ServiceLocator.Current.GetService<INotificationService>()?.Show("Mali Dönem Oluşturuldu", $"{MaliYil} dönemi hazır.", NotificationType.Success);
            await Task.Delay(900);
            Hide();
            return;
        }

        MarkFailed(errorMessage ?? response?.Message ?? "İşlem başarısız oldu.");
    }

    private async Task AdvanceWhileRunningAsync(Task operation)
    {
        var step = 0;
        var lastAdvance = DateTime.UtcNow;
        while (!operation.IsCompleted)
        {
            if (step < Steps.Count && (DateTime.UtcNow - lastAdvance).TotalMilliseconds >= 400)
            {
                lastAdvance = DateTime.UtcNow;
                SetStepStatus(step, SagaStepStatus.Completed);
                step++;
                _activeIndex = step;
                if (step < Steps.Count)
                    SetStepStatus(step, SagaStepStatus.InProgress);
            }
            await Task.Delay(40);
        }
    }

    private void CompleteAllSteps()
    {
        for (var i = 0; i < Steps.Count; i++)
            SetStepStatus(i, SagaStepStatus.Completed);
    }

    private void MarkFailed(string message)
    {
        if (_activeIndex >= 0 && _activeIndex < Steps.Count)
            SetStepStatus(_activeIndex, SagaStepStatus.Failed);
        for (var i = _activeIndex + 1; i < Steps.Count; i++)
        {
            if (Steps[i].Status == SagaStepStatus.InProgress)
                Steps[i].Status = SagaStepStatus.Pending;
        }

        SetStatus("❌ " + message, failed: true);
        StatusIcon.Glyph = "\uE783";
        _running = false;
        IsPrimaryButtonEnabled = true;
        IsSecondaryButtonEnabled = true;
    }
}
