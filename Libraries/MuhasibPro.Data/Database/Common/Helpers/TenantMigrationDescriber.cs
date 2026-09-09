using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MuhasibPro.Data.Contracts.Database.Common.Helpers;

namespace MuhasibPro.Data.Database.Common.Helpers;

/// <summary>
/// Migration sınıfının Up() metodunu boş MigrationBuilder ile çalıştırıp
/// operasyon listesinden Türkçe açıklama üretir. Bilinmeyen ID'de "Şema Değişikliği" döner.
/// </summary>
public class TenantMigrationDescriber : ITenantMigrationDescriber
{
    private readonly ConcurrentDictionary<string, TenantMigrationDescription> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly MethodInfo UpMethod = typeof(Migration).GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic);

    public TenantMigrationDescription Describe(string migrationId)
    {
        if (string.IsNullOrWhiteSpace(migrationId))
            return Unknown(migrationId);
        return _cache.GetOrAdd(migrationId, DescribeCore);
    }

    private TenantMigrationDescription DescribeCore(string migrationId)
    {
        try
        {
            var type = FindMigrationType(migrationId);
            if (type == null || UpMethod == null)
                return Unknown(migrationId);

            var migration = (Migration)Activator.CreateInstance(type);
            var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");
            UpMethod.Invoke(migration, new object[] { builder });

            var details = builder.Operations.Select(FormatOperation).Where(l => l != null).ToList();
            if (details.Count == 0)
                return Unknown(migrationId);

            var tables = GroupTables(builder.Operations);
            return new TenantMigrationDescription
            {
                MigrationId = migrationId,
                IsKnown = true,
                ChangeKind = Classify(builder.Operations),
                Headline = BuildHeadline(builder.Operations, tables),
                Summary = Summarize(builder.Operations),
                Details = details,
                Tables = tables
            };
        }
        catch
        {
            return Unknown(migrationId);
        }
    }

    private static Type FindMigrationType(string migrationId)
    {
        // "20260908_AddTenantIdentity" → "AddTenantIdentity" sınıf adı eşleşmesi.
        var cut = migrationId.IndexOf('_');
        var className = cut >= 0 ? migrationId.Substring(cut + 1) : migrationId;
        return typeof(TenantMigrationDescriber).Assembly.GetTypes()
            .FirstOrDefault(t => !t.IsAbstract
                && typeof(Migration).IsAssignableFrom(t)
                && string.Equals(t.Name, className, StringComparison.OrdinalIgnoreCase));
    }

    private static string Classify(IReadOnlyList<MigrationOperation> ops)
    {
        if (ops.All(o => o is SqlOperation))
            return "Sorgu güncellemesi";
        if (ops.All(o => o is CreateIndexOperation || o is DropIndexOperation))
            return "İndeks güncellemesi";
        if (ops.Any(o => o is AddColumnOperation || o is CreateTableOperation || o is AlterColumnOperation || o is DropColumnOperation || o is DropTableOperation || o is RenameColumnOperation || o is RenameTableOperation))
            return "Tablo güncellemesi";
        return "Şema güncellemesi";
    }

    private static string BuildHeadline(IReadOnlyList<MigrationOperation> ops, List<TenantTableChange> tables)
    {
        var kind = Classify(ops);
        if (kind == "Sorgu güncellemesi" || kind == "İndeks güncellemesi")
            return kind;
        if (tables.Count == 1)
            return tables[0].IsCreated ? $"'{tables[0].Table}' tablosu oluşturuldu" : $"'{tables[0].Table}' tablosu güncellemesi";
        if (tables.Count > 1)
            return $"Tablo güncellemesi ({tables.Count} tablo)";
        return "Şema güncellemesi";
    }

    private static List<TenantTableChange> GroupTables(IReadOnlyList<MigrationOperation> ops)
    {
        var map = new Dictionary<string, TenantTableChange>(StringComparer.OrdinalIgnoreCase);
        TenantTableChange For(string table)
        {
            if (!map.TryGetValue(table, out var change))
            {
                change = new TenantTableChange { Table = table };
                map[table] = change;
            }
            return change;
        }
        foreach (var op in ops)
        {
            switch (op)
            {
                case CreateTableOperation o:
                    var created = For(o.Name);
                    created.IsCreated = true;
                    created.AddedColumns.AddRange(o.Columns.Select(c => c.Name));
                    break;
                case AddColumnOperation o:
                    For(o.Table).AddedColumns.Add(o.Name);
                    break;
                case AlterColumnOperation o:
                    For(o.Table).AlteredColumns.Add(o.Name);
                    break;
                case DropColumnOperation o:
                    For(o.Table).RemovedColumns.Add(o.Name);
                    break;
                case DropTableOperation o:
                    For(o.Name).RemovedColumns.Add("(tablo kaldırıldı)");
                    break;
                case RenameColumnOperation o:
                    For(o.Table).AlteredColumns.Add($"{o.Name} → {o.NewName}");
                    break;
                case RenameTableOperation o:
                    For(o.Name).AlteredColumns.Add($"(tablo {o.NewName} oldu)");
                    break;
            }
        }
        return map.Values.ToList();
    }

    private static string Summarize(IReadOnlyList<MigrationOperation> ops)
    {
        var adds = ops.OfType<AddColumnOperation>().ToList();
        var creates = ops.OfType<CreateTableOperation>().ToList();
        if (adds.Count > 0 && adds.Count == ops.Count)
        {
            var tables = adds.Select(o => o.Table).Distinct().ToList();
            var cols = string.Join(", ", adds.Select(o => o.Name));
            var table = tables.Count == 1 ? tables[0] : string.Join("/", tables);
            return $"{table} tablosuna {adds.Count} kolon eklenecek ({cols})";
        }
        if (creates.Count > 0 && creates.Count == ops.Count)
        {
            var tables = string.Join(", ", creates.Select(o => o.Name));
            return $"{creates.Count} tablo oluşturulacak ({tables})";
        }
        return $"{ops.Count} şema değişikliği";
    }

    private static string FormatOperation(MigrationOperation op)
    {
        return op switch
        {
            AddColumnOperation o => $"{o.Table} tablosuna {o.Name} kolonu eklenecek",
            CreateTableOperation o => $"{o.Name} tablosu oluşturulacak ({o.Columns.Count} kolon)",
            DropColumnOperation o => $"{o.Table} tablosundaki {o.Name} kolonu kaldırılacak",
            DropTableOperation o => $"{o.Name} tablosu kaldırılacak",
            AlterColumnOperation o => $"{o.Table} tablosundaki {o.Name} kolonu güncellenecek",
            RenameColumnOperation o => $"{o.Table} tablosundaki {o.Name} kolonu {o.NewName} olarak yeniden adlandırılacak",
            RenameTableOperation o => $"{o.Name} tablosu {o.NewName} olarak yeniden adlandırılacak",
            CreateIndexOperation o => $"{o.Table} tablosuna {o.Name} indeksi eklenecek",
            DropIndexOperation o => $"{o.Table} tablosundaki {o.Name} indeksi kaldırılacak",
            SqlOperation => "Özel SQL komutu çalıştırılacak",
            _ => $"Şema değişikliği ({op.GetType().Name})"
        };
    }

    private static TenantMigrationDescription Unknown(string migrationId)
    {
        return new TenantMigrationDescription { MigrationId = migrationId ?? string.Empty };
    }
}
