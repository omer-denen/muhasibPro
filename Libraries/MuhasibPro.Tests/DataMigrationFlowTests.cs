using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MuhasibPro.Data.Database.Extensions;
using MuhasibPro.Data.DataContext;
using MuhasibPro.Domain.Models.DatabaseResultModel;
using MuhasibPro.Domain.Models.DatabaseResultModel.DatabaseDiagModel;

namespace MuhasibPro.Tests;

public class DataMigrationFlowTests
{
    private static readonly string[] SistemTables =
    {
        nameof(SistemDbContext.Kullanicilar),
        nameof(SistemDbContext.AppDbVersiyonlar),
        nameof(SistemDbContext.Firmalar),
        nameof(SistemDbContext.MaliDonemler),
        nameof(SistemDbContext.Hesaplar),
        nameof(SistemDbContext.SistemLogs),
    };

    private static (SistemDbContext ctx, SqliteConnection keepAlive) CreateContext()
    {
        var cs = $"Data Source=flow{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var keepAlive = new SqliteConnection(cs);
        keepAlive.Open();
        var options = new DbContextOptionsBuilder<SistemDbContext>().UseSqlite(cs).Options;
        return (new SistemDbContext(options), keepAlive);
    }

    private static DatabaseConnectionAnalysis FakeAnalysis(
        bool isDatabaseExists = true,
        bool canConnect = true,
        bool databaseValid = true,
        bool isEmptyDatabase = false,
        List<string> pendingMigrations = null!)
        => new()
        {
            DatabaseName = "Test.db",
            IsDatabaseExists = isDatabaseExists,
            CanConnect = canConnect,
            DatabaseValid = databaseValid,
            IsEmptyDatabase = isEmptyDatabase,
            PendingMigrations = pendingMigrations ?? new List<string>(),
            AppliedMigrationsCount = 0,
        };

    // ── 1. Core analiz: migrasyonlu sağlıklı DB ───────────────────────
    [Fact]
    public async Task AnalyzeDatabaseCore_SaglikliDb_TumAlanlariDoldurur()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            await ctx.Database.MigrateAsync();

            var analysis = await ctx.GetConnectionFullStateAsync(
                "Sistem.db", true, true, SistemTables, null!);

            analysis.CanConnect.Should().BeTrue();
            analysis.DatabaseValid.Should().BeTrue();
            analysis.HasError.Should().BeFalse();
            analysis.TableCount.Should().Be(SistemTables.Length);
            analysis.IsEmptyDatabase.Should().BeFalse("seed + migration tablo oluşturur");
            analysis.PendingMigrations.Should().BeEmpty();
            analysis.AppliedMigrationsCount.Should().BeGreaterThan(0);
            analysis.CurrentVersion.Should().NotBeNullOrEmpty();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task AnalyzeDatabaseCore_BosDb_Ve_GuncellemeGerekli()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            // Migrate edilmedi → tablolar yok, migration bekliyor
            var analysis = await ctx.GetConnectionFullStateAsync(
                "Sistem.db", true, true, SistemTables, null!);

            analysis.CanConnect.Should().BeTrue();
            analysis.TableCount.Should().Be(0);
            analysis.IsEmptyDatabase.Should().BeTrue();
            analysis.IsUpdateRequired.Should().BeTrue();
            analysis.PendingMigrations.Should().NotBeEmpty();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task AnalyzeDatabaseCore_DosyaYok_HataYokAmaBaglantiYok()
    {
        var (ctx, keepAlive) = CreateContext();
        try
        {
            var analysis = await ctx.GetConnectionFullStateAsync(
                "Olmaz.db", isDatabaseExists: false, databaseValid: false, SistemTables, null!);

            analysis.IsDatabaseExists.Should().BeFalse();
            analysis.Message.Should().Contain("bulunamad");
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    // ── 2. Diagnostics wrapper: Value-alias regresyonu ────────────────
    [Fact]
    public async Task GetDatabaseFullDiagState_MigrasyonluDb_TableCountSifirDegil()
    {
        var (ctx, keepAlive) = CreateContext();
        var reports = new List<AnalysisProgress>();
        IProgress<AnalysisProgress> recorder = new RecordingProgress(reports);
        try
        {
            await ctx.Database.MigrateAsync();

            var diag = await ctx.GetDatabaseFullDiagStateAsync(
                "Sistem.db", true, true, SistemTables, recorder, null!, null!);

            diag.TableCount.Should().Be(SistemTables.Length, "SELECT name AS Value alias olmadan tablolar boş dönerdi");
            diag.DatabaseValid.Should().BeTrue();
            reports.Should().NotBeEmpty();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    private sealed class RecordingProgress : IProgress<AnalysisProgress>
    {
        private readonly List<AnalysisProgress> _list;
        public RecordingProgress(List<AnalysisProgress> list) => _list = list;
        public void Report(AnalysisProgress value) => _list.Add(value);
    }

    // ── 3. Migration akışı: sahte rollback regresyonu ─────────────────
    [Fact]
    public async Task ExecuteMigrations_BosDb_MigrateEderVeRestoreCagirmaz()
    {
        var (ctx, keepAlive) = CreateContext();
        int restoreCalls = 0, backupCalls = 0;
        try
        {
            var analysis = await ctx.GetConnectionFullStateAsync(
                "Sistem.db", true, true, SistemTables, null!);

            var result = await ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: () => { restoreCalls++; return Task.FromResult(true); },
                backupAction: () => { backupCalls++; return Task.FromResult(true); },
                logger: null!);

            result.IsSuccess.Should().BeTrue();
            result.IsRolledBack.Should().BeFalse("sağlıklı migration'da sahte rollback olmamalı");
            result.BackupTaken.Should().BeFalse("boş veritabanında yedek gerekmez");
            result.HasError.Should().BeFalse();
            restoreCalls.Should().Be(0);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task ExecuteMigrations_VeriliDb_BackupAlir()
    {
        var (ctx, keepAlive) = CreateContext();
        int backupCalls = 0, restoreCalls = 0;
        try
        {
            await ctx.Database.MigrateAsync();

            // Verili + bekleyen migration varmış gibi fabrikasyon analiz
            var analysis = FakeAnalysis(isEmptyDatabase: false, pendingMigrations: new List<string> { "_FakePending" });

            var result = await ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: () => { restoreCalls++; return Task.FromResult(true); },
                backupAction: () => { backupCalls++; return Task.FromResult(true); },
                logger: null!);

            backupCalls.Should().Be(1);
            result.BackupTaken.Should().BeTrue();
            restoreCalls.Should().Be(0, "doğrulama geçti, restore gerekmez");
            result.IsRolledBack.Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    // ── 4. Backup false → sonsuz döngü regresyonu (hızlı fail, 3 deneme) ──
    [Fact]
    public async Task ExecuteMigrations_BackupHepFalse_3DenemeVeFail()
    {
        var (ctx, keepAlive) = CreateContext();
        int backupCalls = 0;
        try
        {
            await ctx.Database.MigrateAsync();
            var analysis = FakeAnalysis(isEmptyDatabase: false, pendingMigrations: new List<string> { "_FakePending" });

            var task = ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: null!,
                backupAction: () => { backupCalls++; return Task.FromResult(false); },
                logger: null!);

            var result = await task.WaitAsync(TimeSpan.FromSeconds(30));

            result.HasError.Should().BeTrue();
            result.Message.Should().Contain("Yedek alınamadığı");
            backupCalls.Should().Be(3, "maks 3 deneme (sonsuz döngü regresyonu)");
            result.IsRolledBack.Should().BeFalse();
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    // ── 5. Geçersiz DB → restore → yeniden analiz ─────────────────────
    [Fact]
    public async Task ExecuteMigrations_GecersizDb_RestoreVeYenidenAnaliz()
    {
        var (ctx, keepAlive) = CreateContext();
        int restoreCalls = 0;
        try
        {
            await ctx.Database.MigrateAsync();

            // DatabaseValid=false → restore tetiklenmeli
            var analysis = FakeAnalysis(databaseValid: false);

            var result = await ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: () => { restoreCalls++; return Task.FromResult(true); },
                backupAction: null!,
                logger: null!);

            restoreCalls.Should().Be(1);
            result.IsRolledBack.Should().BeTrue();
            result.DatabaseValid.Should().BeTrue("restore sonrası yeniden analiz geçerli DB görmeli");
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    [Fact]
    public async Task ExecuteMigrations_GecersizDb_RestoreBasarsiz_Hata()
    {
        var (ctx, keepAlive) = CreateContext();
        int restoreCalls = 0;
        try
        {
            await ctx.Database.MigrateAsync();
            var analysis = FakeAnalysis(databaseValid: false);

            var result = await ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: () => { restoreCalls++; return Task.FromResult(false); },
                backupAction: null!,
                logger: null!);

            result.HasError.Should().BeTrue();
            result.Message.Should().Contain("geri yüklenemediği");
            restoreCalls.Should().Be(3);
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }

    // ── 6. Oturum 127: retry sayısı ayardan taşınır (maxAttempts threading) ──
    [Fact]
    public async Task ExecuteMigrations_Retry2_IkiDenemeVeFail()
    {
        var (ctx, keepAlive) = CreateContext();
        int backupCalls = 0;
        try
        {
            await ctx.Database.MigrateAsync();
            var analysis = FakeAnalysis(isEmptyDatabase: false, pendingMigrations: new List<string> { "_FakePending" });

            var result = await ctx.ExecuteMigrationsWithBackupCheckAsync(
                analysis,
                restoreAction: null!,
                backupAction: () => { backupCalls++; return Task.FromResult(false); },
                logger: null!,
                maxAttempts: 2);

            result.HasError.Should().BeTrue();
            backupCalls.Should().Be(2, "TenantSettings.MigrationRetry değeri deneme sayısına taşınır");
        }
        finally { keepAlive.Dispose(); ctx.Dispose(); }
    }
}