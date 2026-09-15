using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices;
using MuhasibPro.Business.Contracts.DatabaseServices.TenantDatabaseServices.Common;
using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.SistemServices.LogServices;
using MuhasibPro.Business.Contracts.Installation;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Domain.Models;
using MuhasibPro.Business.ResultModels.TenantResultModels;
using MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService.Common;
using MuhasibPro.Business.Services.SistemServices.LogServices;
using MuhasibPro.Data.Contracts.Database.Common;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;
using MuhasibPro.Data.Contracts.Repository.Common.BaseRepo;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Utilities.Responses;

namespace MuhasibPro.Business.Services.DatabaseServices.TenantDatabaseService
{
    public class TenantSQLiteDatabaseService : ITenantSQLiteDatabaseService
    {
        private readonly ILogService _logService;
        private readonly IMaliDonemService _maliDonemService;
        private readonly IFirmaService _firmaService;
        private readonly ILogger<TenantSQLiteDatabaseService> _logger;
        private readonly IApplicationPaths _applicationPaths;
        private readonly ITenantSQLiteDatabaseLifecycleService _lifecycleService;
        private readonly ITenantSQLiteSelectionService _selectionService;
        private readonly IMaliDonemSagaStep _maliDonemSaga;
        private readonly ITenantDatabaseSagaStep _tenantDatabaseSagaStep;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMakineKimligiProvider _makineProvider;
        private readonly IKurulumKayitService _kurulumService;
        private readonly IAppDbContextFactory _appDbContextFactory;
        private readonly ILocalSettingsService _localSettings;
        private readonly ITenantSettingsProvider _settingsProvider;

        public TenantSQLiteDatabaseService(
            ILogService logService,
            IMaliDonemService maliDonemService,
            IFirmaService firmaService,
            ILogger<TenantSQLiteDatabaseService> logger,
            ILocalSettingsService localSettings,
            IApplicationPaths applicationPaths,

            ITenantSQLiteDatabaseLifecycleService lifecycleService,
            ITenantSQLiteSelectionService selectionService,
            IMaliDonemSagaStep maliDonemSaga,
            ITenantDatabaseSagaStep tenantDatabaseSagaStep,
            IAuthenticationService authenticationService,
            IMakineKimligiProvider makineProvider,
            IKurulumKayitService kurulumService,
            IAppDbContextFactory appDbContextFactory,
            ITenantSettingsProvider settingsProvider = null)
        {
            _logService = logService;
            _maliDonemService = maliDonemService;
            _firmaService = firmaService;
            _logger = logger;

            _applicationPaths = applicationPaths;

            _lifecycleService = lifecycleService;
            _selectionService = selectionService;
            _maliDonemSaga = maliDonemSaga;
            _tenantDatabaseSagaStep = tenantDatabaseSagaStep;
            _authenticationService = authenticationService;
            _makineProvider = makineProvider;
            _kurulumService = kurulumService;
            _appDbContextFactory = appDbContextFactory;
            _localSettings = localSettings;
            _settingsProvider = settingsProvider;
        }

        public async Task<ApiDataResponse<TenantCreationResult>> CreateNewTenantDatabaseAsync(
            TenantCreationRequest request)
        {
            _logger.LogInformation(
                "Mali Dönem Veribanı oluşturma işlemi başlatıldı - FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                request.FirmaId,
                request.MaliYil);

            var result = new TenantCreationResult
            {
                FirmaId = request.FirmaId,
                MaliYil = request.MaliYil,
                DatabaseName = request.DatabaseName,
                CreateCompleted = false,
            };
            result.StartStep(TenantCreationStep.IslemBaslatildi);
            var saga = new TenantOperationSaga(_logger);
            if (request.OlusturanRol == null && _authenticationService.IsAuthenticated)
            {
                try { request.OlusturanRol = MuhasibPro.Domain.Entities.SistemEntity.KullaniciRolTip.Yönetici; } catch { }
            }
            if (request.FirmaId <= 0)
                return ApiDataExtensions.ErrorResponse(result, "Firma Id geçersiz");
            // Mali Yıl Validasyonu
            result.StartStep(TenantCreationStep.MaliYilGecerlilikKontrolu);

            var maliYilResponse = TenantHelperExtensions.ValidateMaliYil(request.MaliYil);
            if (!maliYilResponse.Success || !maliYilResponse.Data)
            {
                result.CompleteStep(CreationStepStatus.Hata, maliYilResponse.Message);
                result.MarkAsError(maliYilResponse.Message);
                return new ErrorApiDataResponse<TenantCreationResult>(data: result, message: maliYilResponse.Message);
            }

            result.MaliYil = request.MaliYil;
            result.CompleteStep(CreationStepStatus.Tamamlandi, maliYilResponse.Message);

            result.StartStep(TenantCreationStep.MaliDonemZatenVarMiKontrolu);
            var maliDonemExist = await _maliDonemService.ValidateMaliDonemExistsAsync(request.FirmaId, result.MaliYil);
            if (!maliDonemExist.Success)
            {
                result.CompleteStep(CreationStepStatus.Uyari, maliDonemExist.Message);
                result.MarkAsError(maliDonemExist.Message);
                return new ErrorApiDataResponse<TenantCreationResult>(data: result, message: maliDonemExist.Message);
            }
            result.CompleteStep(CreationStepStatus.Tamamlandi, "Mali Dönem kontrolü tamamlandı");
            
            try
            {
                // Firma Validasyon
                result.StartStep(TenantCreationStep.FirmaBilgileriKontrolu);

                var firmaResponse = await _firmaService.ValidateFirmaAsync(request.FirmaId);
                if (!firmaResponse.Success || firmaResponse.Data == null)
                {
                    result.CompleteStep(CreationStepStatus.Hata, firmaResponse.Message);
                    result.MarkAsError(firmaResponse.Message);
                    return new ErrorApiDataResponse<TenantCreationResult>(data: result, message: firmaResponse.Message);
                }

                result.FirmaId = firmaResponse.Data.Id;
                result.CompleteStep(CreationStepStatus.Tamamlandi, firmaResponse.Message);

                // Firma Kodu kontrolü ve Veritabanı adı oluşturma
                result.StartStep(TenantCreationStep.VeritabaniAdiOlusturuluyor);

                if (string.IsNullOrWhiteSpace(firmaResponse.Data.FirmaKodu))
                {
                    result.CompleteStep(CreationStepStatus.Hata, "Firma Kodu boş olamaz");
                    result.MarkAsError("Firma kodu bilgilerine ulaşılamadı");
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        data: result,
                        message: "Firma Kodu bilgilerine ulaşılamadı");
                }

                var databaseNameResponse = _applicationPaths.GenerateDatabaseName(
                    firmaResponse.Data.FirmaKodu,
                    request.MaliYil);
                if (!databaseNameResponse.Success || string.IsNullOrWhiteSpace(databaseNameResponse.Data))
                {
                    result.CompleteStep(CreationStepStatus.Hata, databaseNameResponse.Message);
                    result.MarkAsError(databaseNameResponse.Message);
                    return new ErrorApiDataResponse<TenantCreationResult>(
                        data: result,
                        message: databaseNameResponse.Message);
                }

                result.DatabaseName = databaseNameResponse.Data;
                // Caller tarafında kullanılacak request.DatabaseName'i güncelle
                request.DatabaseName = result.DatabaseName;

                result.CompleteStep(CreationStepStatus.Tamamlandi, databaseNameResponse.Message);

                // Mali Dönem Kaydı
                result.StartStep(TenantCreationStep.MaliDonemKaydiOlusturuluyor);

                var maliDonem = await _maliDonemSaga.CreateNewMaliDonemAsync(saga, request);
                if (!maliDonem.Success || maliDonem.Data == null)
                {
                    result.CompleteStep(CreationStepStatus.Hata, maliDonem.Message);
                    result.MarkAsError(maliDonem.Message);
                    return new ErrorApiDataResponse<TenantCreationResult>(data: result, message: maliDonem.Message);
                }

                result.MaliDonemId = maliDonem.Data.MaliDonemId;
                result.CompleteStep(CreationStepStatus.Tamamlandi, maliDonem.Message);

                // Veritabanı oluştur
                /// <summary>
                /// ileriye dönük bir property.  ViewModel tarafından otomatik true gönderilecek Veritabanı oluşturmak
                /// şimdilik zorunlu.
                /// </summary>
                if (request.AutoCreateDatabase)
                {
                    result.StartStep(TenantCreationStep.VeritabaniDosyasiOlusturuluyor);

                    var maliVeritabani = await _tenantDatabaseSagaStep
                        .CreateTenantDatabaseAsync(saga, result.DatabaseName);
                    if (!maliVeritabani.Success || maliVeritabani.Data == null)
                    {
                        result.CompleteStep(CreationStepStatus.Hata, maliVeritabani.Message);
                        result.MarkAsError(maliVeritabani.Message);
                        return new ErrorApiDataResponse<TenantCreationResult>(
                            data: result,
                            message: maliVeritabani.Message);
                    }

                    result.DatabaseCreated = true;
                    result.CompleteStep(CreationStepStatus.Tamamlandi, maliVeritabani.Message);
                    await PersistTenantFileSizeAsync(result);
                    await StampTenantIdentityAsync(result, request);
                }
                else
                {
                    _logger.LogInformation("Veritabanı oluşturma atlandı (AutoCreateDatabase=false)");
                    result.DatabaseCreated = false;
                    await _logService.SistemLogService
                        .SistemLogInformationAsync(
                            "Mali Dönem İşlemleri",
                            "Mali Dönem Veritabanı Oluştur",
                            "Veritabanı oluşturma işlemi kullanıcı tarafından atlandı",
                            "Veritabanı oluşturma işlemi atlandı, sadece Mali Dönem kaydı oluşturuldu.");
                }

                result.CreateCompleted = true;
                result.CompleteStep(CreationStepStatus.Tamamlandi,"Veritabanı ve mali dönem başarıyla oluşturuldu");
                result.MarkAsSuccess($"{firmaResponse.Data.KisaUnvani} - {result.MaliYil} mali dönemi oluşturuldu");
                

                return new SuccessApiDataResponse<TenantCreationResult>(
                    data: result,
                    message: $"{firmaResponse.Data.KisaUnvani} - {result.MaliYil} mali dönemi oluşturuldu");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Saga geri alımı: yarım kalmış dosya/satır temizlenir (A8 fix).
                try { await saga.CompensateAllAsync(); }
                catch (Exception compEx) { _logger.LogWarning(compEx, "Create saga kompansasyonu başarısız"); }

                result.MarkAsError($"Beklenmeyen hata: {ex.Message}");
                _logger.LogError(
                    ex,
                    "Mali Dönem oluşturma BAŞARISIZ - FirmaId: {FirmaId}, MaliYil: {MaliYil}",
                    request.FirmaId,
                    request.MaliYil);
                await _logService.SistemLogService
                    .SistemLogExceptionAsync("Mali Dönem İşlemleri", "Mali Dönem Oluşturma", ex);
                return new ErrorApiDataResponse<TenantCreationResult>(
                    result,
                    $"Mali Dönem oluşturma hatası: {ex.Message}");
            }
        }

        private const string maliDonemIDNull = "Mali Dönem Id boş olamaz";
        private const string databaseNameNull = "Veritabanı adı boş olamaz";

        private async Task StampTenantIdentityAsync(TenantCreationResult result, TenantCreationRequest request)
        {
            try
            {
                if (result == null || string.IsNullOrWhiteSpace(result.DatabaseName) || result.MaliDonemId <= 0)
                    return;
                var baglanti = await BaglantiDegerleriAsync();
                using var ctx = _appDbContextFactory.CreateDbContext(
                    result.DatabaseName, baglanti.CommandTimeoutSec, baglanti.BusyTimeoutMs, baglanti.Pooling);
                var version = ctx.TenantDatabaseVersiyonlar.FirstOrDefault(v => v.DatabaseName == result.DatabaseName);
                if (version == null) return;
                bool changed = false;
                if (version.FirmaId == null) { version.FirmaId = result.FirmaId; changed = true; }
                if (version.MaliDonemId == null) { version.MaliDonemId = result.MaliDonemId; changed = true; }
                if (version.OlusturanKullaniciId == null && request.OlusturanRol != null) { version.OlusturanKullaniciId = request.OlusturanRol; changed = true; }
                if (string.IsNullOrWhiteSpace(version.MakineId))
                {
                    try { version.MakineId = await _makineProvider.GetMachineIdAsync(); changed = true; } catch { }
                }
                if (string.IsNullOrWhiteSpace(version.KurulumId))
                {
                    try { var k = await _kurulumService.GetOrCreateAsync(); version.KurulumId = k.KurulumId; changed = true; } catch { }
                }
                if (changed) await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tenant kimlik damgasi yazilamadi: {DatabaseName}", result?.DatabaseName);
            }
        }

        public async Task<int> BackfillMissingTenantIdentitiesAsync()
        {
            int patched = 0;
            try
            {
                // Sistem modülüyle ortak altyapı üzerinden konuşulur: somut DbContext'e dokunulmaz.
                var maliDonemler = await TumMaliDonemleriYukleAsync();
                var kurulum = await _kurulumService.GetOrCreateAsync();
                var makineId = await _makineProvider.GetMachineIdAsync();
                foreach (var md in maliDonemler)
                {
                    try
                    {
                        var dbName = md.DatabaseName;
                        if (string.IsNullOrWhiteSpace(dbName)) continue;
                        if (!_applicationPaths.TenantDatabaseFileExists(dbName)) continue;
                        var baglanti = await BaglantiDegerleriAsync();
                        using var ctx = _appDbContextFactory.CreateDbContext(
                            dbName, baglanti.CommandTimeoutSec, baglanti.BusyTimeoutMs, baglanti.Pooling);
                        var ver = ctx.TenantDatabaseVersiyonlar.FirstOrDefault(v => v.DatabaseName == dbName);
                        if (ver == null) continue;
                        bool ch = false;
                        if (ver.FirmaId == null) { ver.FirmaId = md.FirmaId; ch = true; }
                        if (ver.MaliDonemId == null) { ver.MaliDonemId = md.Id; ch = true; }
                        if (ver.OlusturanKullaniciId == null) { ver.OlusturanKullaniciId = MuhasibPro.Domain.Entities.SistemEntity.KullaniciRolTip.Yönetici; ch = true; }
                        if (string.IsNullOrWhiteSpace(ver.MakineId)) { ver.MakineId = makineId; ch = true; }
                        if (string.IsNullOrWhiteSpace(ver.KurulumId)) { ver.KurulumId = kurulum.KurulumId; ch = true; }
                        if (ch) { await ctx.SaveChangesAsync(); patched++; }
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Backfill kimlik damgasi hatasi"); }
            return patched;
        }

        /// <summary>Kimlik kaybı onarımı (karışık damga): verilen dönemlerin kurulum kimliğini
        /// güncel kimliğe eşitler — makine aynı olduğu için veri bu kurulumun sayılır.</summary>
        public async Task<int> ReAlignTenantKurulumIdsAsync(IReadOnlyCollection<string> databaseNames)
        {
            int patched = 0;
            try
            {
                if (databaseNames == null || databaseNames.Count == 0)
                    return 0;

                var kurulum = await _kurulumService.GetOrCreateAsync();
                foreach (var dbName in databaseNames)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(dbName)) continue;
                        if (!_applicationPaths.TenantDatabaseFileExists(dbName)) continue;
                        var baglanti = await BaglantiDegerleriAsync();
                        using var ctx = _appDbContextFactory.CreateDbContext(
                            dbName, baglanti.CommandTimeoutSec, baglanti.BusyTimeoutMs, baglanti.Pooling);
                        var ver = ctx.TenantDatabaseVersiyonlar.FirstOrDefault(v => v.DatabaseName == dbName);
                        if (ver == null) continue;
                        if (string.Equals(ver.KurulumId, kurulum.KurulumId, StringComparison.OrdinalIgnoreCase)) continue;
                        ver.KurulumId = kurulum.KurulumId;
                        await ctx.SaveChangesAsync();
                        patched++;
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Kurulum kimligi esitleme hatasi"); }
            return patched;
        }

        /// <summary>Doğrudan tenant context açılışları için bağlantı değerleri
        /// (Oturum 127). Sağlayıcı yoksa/okunamazsa hepsi null → fabrika varsayılanları.</summary>
        private async Task<(int? CommandTimeoutSec, int? BusyTimeoutMs, bool? Pooling)> BaglantiDegerleriAsync()
        {
            try
            {
                if (_settingsProvider == null)
                    return (null, null, null);
                var ayar = await _settingsProvider.GetAsync();
                if (ayar == null)
                    return (null, null, null);
                return (ayar.GetCommandTimeoutSec(), ayar.GetBusyTimeoutMs(), ayar.Pooling);
            }
            catch
            {
                return (null, null, null);
            }
        }

        /// <summary>Sistem.db MaliDonem satırları — ortak sözleşme (IMaliDonemService) üzerinden sayfalı okunur.</summary>
        private async Task<IReadOnlyList<Business.DTOModel.SistemModel.MaliDonemModel>> TumMaliDonemleriYukleAsync()
        {
            int sayfa = new TenantSettings().GetBackfillSayfaBoyutu();
            try
            {
                var ayar = await _localSettings.ReadSettingAsync<TenantSettings>(TenantSettings.SettingsKey);
                if (ayar != null)
                    sayfa = ayar.GetBackfillSayfaBoyutu();
            }
            catch { /* model varsayılanı korunur */ }
            var tumu = new List<Business.DTOModel.SistemModel.MaliDonemModel>();
            int atlanan = 0;
            while (true)
            {
                var resp = await _maliDonemService.GetMaliDonemlerPageAsync(
                    atlanan, sayfa, new Domain.Common.DataRequest<Domain.Entities.SistemEntity.MaliDonem>());
                var liste = resp?.Data;
                if (liste == null || liste.Count == 0)
                    break;
                tumu.AddRange(liste);
                if (liste.Count < sayfa)
                    break;
                atlanan += sayfa;
            }
            return tumu;
        }

        /// <summary>
        /// Tenant DB dosyası oluşturulduktan sonra Global.db MaliDonem satırına
        /// güncel dosya boyutunu işler — kart vitrini (Boyut) satırdan okuduğu için
        /// yeni dönem kartında "-" kalmaz.
        /// </summary>
        private async Task PersistTenantFileSizeAsync(TenantCreationResult result)
        {
            try
            {
                if (result == null || result.MaliDonemId <= 0 || string.IsNullOrWhiteSpace(result.DatabaseName))
                    return;

                var dbPath = _applicationPaths.GetTenantDatabaseFilePath(result.DatabaseName);
                if (!File.Exists(dbPath))
                    return;

                var donemResponse = await _maliDonemService.GetByMaliDonemIdAsync(result.MaliDonemId);
                if (!donemResponse.Success || donemResponse.Data == null)
                    return;

                donemResponse.Data.TenantDetails ??= new TenantDetailsModel();
                donemResponse.Data.TenantDetails.DosyaBoyutu = new FileInfo(dbPath).Length;
                await _maliDonemService.UpdateMaliDonemAsync(donemResponse.Data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tenant dosya boyutu Global.db satırına işlenemedi: {DatabaseName}", result?.DatabaseName);
            }
        }

        public async Task<ApiDataResponse<TenantDeletingResult>> DeleteTenantDatabaseAsync(
            TenantDeletingRequest request)
        {
            _logger.LogInformation(
                "Veritabanı silme işlemi başlatıldı - MaliDonemId: {MaliDonemId}, DatabaseName: {DatabaseName} ",
                request.MaliDonemId,
                request.DatabaseName);
            var result = new TenantDeletingResult
            {
                MaliDonemId = request.MaliDonemId,
                DatabaseName = request.DatabaseName,
                BackupCreateCompleted = false,
                BackupDeleteCompleted = false,
                DeletedBackupCount = 0,
                IsCurrentTenantDeletingBeforeBackup = false,
                MaliDonemDeleted = false,
                DatabaseDeleted = false,
                DeleteCompleted = false,
            };
            result.StartStep(TenantDeletionStep.IslemBaslatildi);
            var saga = new TenantOperationSaga(_logger);
            if (request.MaliDonemId <= 0)
            {
                result.CompleteStep(DeletionStepStatus.Hata, maliDonemIDNull);
                result.MarkAsError($"Gönderilen Mali Dönem Id : '{request.MaliDonemId}' , {maliDonemIDNull} ");
                return ApiDataExtensions.ErrorResponse(result, maliDonemIDNull);
            }
            result.CompleteStep(DeletionStepStatus.Tamamlandi, "Mali Dönem doğrulaması tamamlandı");
            if (request.DatabaseName == null)
            {
                result.CompleteStep(DeletionStepStatus.Hata, databaseNameNull);
                result.MarkAsError($"Gönderilen veritabanı : '{request.DatabaseName}', {databaseNameNull} ");
                return ApiDataExtensions.ErrorResponse(result, databaseNameNull);
            }
            result.CompleteStep(DeletionStepStatus.Tamamlandi, "Veritabanı adı doğrulaması tamamlandı");
            // Silme doğrulaması satır-düzeyinde yapılır: dosya elle silinmiş olabilir — dosya
            // yokluğu silmeyi engellemez (yetim satır temizlenebilmeli). Satır varlığı + ad eşleşmesi yeterli.
            var maliDonemResponse = await _maliDonemService.GetByMaliDonemIdAsync(request.MaliDonemId);
            result.CompleteStep(DeletionStepStatus.Calisiyor, maliDonemResponse.Message);
            if (!maliDonemResponse.Success || maliDonemResponse.Data == null || !string.Equals(maliDonemResponse.Data.DatabaseName, request.DatabaseName, StringComparison.OrdinalIgnoreCase))
            {
                var message = maliDonemResponse.Message;
                result.CompleteStep(DeletionStepStatus.Hata, message);
                result.MarkAsError(message);
                return ApiDataExtensions.ErrorResponse(result, message);
            }
            result.CompleteStep(DeletionStepStatus.Tamamlandi, "Mali Dönem kaydı doğrulandı, silme işlemi devam ediyor");
            result.MaliDonemId = maliDonemResponse.Data.Id;
            result.DatabaseName = maliDonemResponse.Data.DatabaseName;
            try
            {
                if (request.IsDeleteDatabase)
                {
                    result.CompleteStep(DeletionStepStatus.Calisiyor, "Veritabanı silme işlemi başlatıldı");
                    result.StartStep(TenantDeletionStep.VeritabaniDosyasiSiliniyor);

                    var deletingDatabaseResponse = await _tenantDatabaseSagaStep
                        .DeleteTenantDatabaseAsync(saga, request);
                    if (!deletingDatabaseResponse.Success || !deletingDatabaseResponse.Data.DatabaseDeleted)
                    {
                        var message = deletingDatabaseResponse.Message;
                        result.CompleteStep(DeletionStepStatus.Hata, message);
                        result.MarkAsError(message);
                        return ApiDataExtensions.ErrorResponse(result, message);
                    }
                    var response = deletingDatabaseResponse.Data;
                    result.DatabaseDeleted = response.DatabaseDeleted;
                    result.CompleteStep(DeletionStepStatus.Tamamlandi, deletingDatabaseResponse.Message);

                    if (request.IsCurrentTenantDeletingBeforeBackup)
                    {                    
                        result.StartStep(TenantDeletionStep.VeritabaniSilmedenOnceYedekAliniyor);
                        if (response.BackupCreateCompleted)
                        {
                            result.BackupCreateCompleted = response.BackupCreateCompleted;
                            result.IsCurrentTenantDeletingBeforeBackup = response.IsCurrentTenantDeletingBeforeBackup;
                            result.BackupFilePath = response.BackupFilePath;
                            result.CompleteStep(DeletionStepStatus.Tamamlandi, "Veritabanı başarıyla yedeklendi");
                            result.MarkAsSuccess("Veritabanı başarıyla yedeklendi");
                        }
                        else
                        {
                            result.BackupCreateCompleted = false;
                            result.IsCurrentTenantDeletingBeforeBackup = false;
                            result.BackupFilePath = null;
                            result.CompleteStep(DeletionStepStatus.Hata, "Veritabanı yedek alma işlemi başarısız");
                            result.MarkAsError(deletingDatabaseResponse.Message);
                        }
                        
                    }
                    else
                    {
                        result.CompleteStep(DeletionStepStatus.Uyari, "Veritabanı silmeden önce yedek alma işlemi kullanıcı tarafından atlandı");
                    }

                    if (request.DeleteAllTenantBackup)
                    {
                        // Saga zaten yedekleri sildi (DeleteTenantDatabase:104-113).
                        // Sonuçları üst result'a aktar.
                        result.BackupDeleteCompleted = response.BackupDeleteCompleted;
                        result.DeletedBackupCount = response.DeletedBackupCount;
                        result.DeletedBackupFiles = response.DeletedBackupFiles;

                        if (response.BackupDeleteCompleted || (response.DeletedBackupFiles?.Count ?? 0) > 0)
                        {
                            result.StartStep(TenantDeletionStep.VeritabaniYedekleriSiliniyor);
                            result.CompleteStep(DeletionStepStatus.Tamamlandi,
                                $"Veritabanı yedekleri silindi ({response.DeletedBackupCount} dosya)");
                        }
                        else
                        {
                            result.CompleteStep(DeletionStepStatus.Tamamlandi, "Silinecek veritabanı yedeği bulunamadı");
                        }
                    }
                    else
                    {
                        result.CompleteStep(DeletionStepStatus.Uyari, "Veritabanı yedeklerini silme işlemi kullanıcı tarafından atlandı");
                    }
                }
                else
                {
                    _logger.LogInformation("Veritabanı silme işlemi atlandı (IsDeleteDatabase=false)");
                    result.DatabaseDeleted = false;
                    await _logService.SistemLogService
                        .SistemLogInformationAsync(
                            "Mali Dönem İşlemleri",
                            "Mali Dönem Veritabanı Silme",
                            "Veritabanı Silme işlemi kullanıcı tarafından atlandı",
                            "Veritabanı Silme işlemi atlandı, sadece Mali Dönem kaydı silinecek");
                    result.CompleteStep(
                        DeletionStepStatus.Uyari,
                        "Veritabanı silme işlemi kullanıcı tarafından atlandı");
                }
                if (request.IsDeleteMaliDonem)
                {
                    result.StartStep(TenantDeletionStep.MaliDonemKaydiSiliniyor);
                    var deleteMaliDonemRecord = await _maliDonemSaga
                        .DeleteMaliDonemAsync(saga, request);
                    if (!deleteMaliDonemRecord.Success || !deleteMaliDonemRecord.Data.MaliDonemDeleted)
                    {
                        var message = deleteMaliDonemRecord.Message;
                        result.CompleteStep(DeletionStepStatus.Hata, message);
                        result.MarkAsError(message);
                        return ApiDataExtensions.ErrorResponse(deleteMaliDonemRecord.Data, message);
                    }
                    var response = deleteMaliDonemRecord.Data;
                    if (response.MaliDonemDeleted)
                    {
                        result.MaliDonemDeleted = true;
                        result.CompleteStep(DeletionStepStatus.Tamamlandi, deleteMaliDonemRecord.Message);
                    }
                }
                else
                {
                    await _logService.SistemLogService
                        .SistemLogInformationAsync(
                            "Mali Dönem İşlemleri",
                            "Mali Dönem Kaydı Silme",
                            "Mali Dönem Kaydı Silme işlemi kullanıcı tarafından atlandı",
                            "Mali Dönem Kaydı işlemi atlandı, sadece veritabanı silinecek");
                    result.CompleteStep(
                        DeletionStepStatus.Uyari,
                        "Mali Dönem kaydı silme işlemi kullanıcı tarafından atlandı");
                }

                // Sonuç: gerçekten bir işlem yapıldıysa başarı, aksi halde uyarı.
                bool gercekIslemYapildi = result.DatabaseDeleted || result.MaliDonemDeleted || result.BackupDeleteCompleted;
                if (gercekIslemYapildi)
                {
                    result.CompleteStep(DeletionStepStatus.Tamamlandi, "Silme işlemi başarıyla tamamlandı");
                    result.DeleteCompleted = true;
                    result.MarkAsSuccess("Silme işlemi başarılı");
                    return ApiDataExtensions.SuccessResponse(result, "Silme işlemi başarıyla tamamlandı");
                }
                else
                {
                    result.CompleteStep(DeletionStepStatus.Uyari, "Hiçbir silme işlemi gerçekleştirilmedi (tüm seçimler kullanıcı tarafından atlandı)");
                    result.DeleteCompleted = false;
                    return ApiDataExtensions.SuccessResponse(result, "Silme işlemi tamamlandı — hiçbir veri silinmedi");
                }
            }
            catch (Exception ex)
            {
                result.MarkAsError($"Beklenmeyen hata: {ex.Message}");
                _logger.LogError(
                    ex,
                    "Mali Dönem Silme BAŞARISIZ - MaliDonemId: {MaliDonemId}, Veritabanı: {DatabaseName}",
                    request.MaliDonemId,
                    request.DatabaseName);
                await _logService.SistemLogService
                    .SistemLogExceptionAsync("Mali Dönem İşlemleri", "Mali Dönem Silme", ex);
                return ApiDataExtensions.ErrorResponse(result, $"Mali Dönem ve Veritabanı silme hatası: {ex.Message}");
            }
        }


        public Task<ApiDataResponse<DatabaseConnectionAnalysis>> GetTenantDatabaseStateAsync(string databaseName) => _lifecycleService
            .GetTenantDatabaseStateAsync(databaseName);

    

        public async Task<ApiDataResponse<TenantContext>> SwitchTenantAsync(string databaseName) => await _selectionService
            .SwitchTenantAsync(databaseName);

        public async Task<(bool isValid, string Message)> ValidateTenantDatabaseAsync(string databaseName) => await _lifecycleService
            .ValidateTenantDatabaseAsync(databaseName);
    }
}

