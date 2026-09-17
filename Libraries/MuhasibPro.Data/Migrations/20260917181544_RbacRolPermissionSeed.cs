using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasibPro.Data.Migrations
{
    /// <summary>
    /// Faz 6.85 K1 — rol → izin matrisi seed'i.
    /// <para>EF <c>InsertData</c> yerine <c>INSERT OR IGNORE</c> kullanılır: mevcut veritabanlarında
    /// elle/geçici eklenmiş <c>RolPermission</c> satırlarıyla çakışmaz (PK: RolId+PermissionId),
    /// migration idempotent olur. Model tarafı <c>HasData</c> ile tanımlıdır (snapshot tutarlı).</para>
    /// </summary>
    public partial class RbacRolPermissionSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 241341 = Yönetici (tüm izinler) · 241342 = Kullanıcı (temel görüntüleme + AI yardım).
            migrationBuilder.Sql(
                """
                INSERT OR IGNORE INTO "RolPermission" ("RolId", "PermissionId") VALUES
                (241341, 100), (241341, 101), (241341, 102), (241341, 103),
                (241341, 200), (241341, 201), (241341, 202), (241341, 203),
                (241341, 300), (241341, 301), (241341, 302), (241341, 303), (241341, 304), (241341, 305),
                (241341, 400), (241341, 401), (241341, 402), (241341, 403), (241341, 404),
                (241341, 500), (241341, 501), (241341, 502), (241341, 503),
                (241341, 600), (241341, 601), (241341, 602), (241341, 603),
                (241341, 700), (241341, 701), (241341, 702), (241341, 703),
                (241341, 800), (241341, 801), (241341, 802), (241341, 803),
                (241341, 900), (241341, 901), (241341, 902), (241341, 903),
                (241341, 1000), (241341, 1001), (241341, 1002), (241341, 1003),
                (241341, 1100), (241341, 1101), (241341, 1102), (241341, 1103),
                (241341, 1200), (241341, 1201), (241341, 1202), (241341, 1203),
                (241341, 1300), (241341, 1301), (241341, 1302), (241341, 1303),
                (241341, 1400), (241341, 1401), (241341, 1402), (241341, 1403),
                (241341, 1500), (241341, 1501), (241341, 1600), (241341, 1601),
                (241341, 1700), (241341, 1800), (241341, 1900), (241341, 2000),
                (241341, 2100), (241341, 2101), (241341, 2102), (241341, 2103), (241341, 2104), (241341, 2105),
                (241341, 2200), (241341, 2201), (241341, 2300),
                (241342, 100), (241342, 200), (241342, 300), (241342, 400),
                (241342, 500), (241342, 600), (241342, 700), (241342, 800),
                (241342, 900), (241342, 1000), (241342, 1100), (241342, 1200),
                (241342, 1300), (241342, 1400), (241342, 1500), (241342, 2300);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // K1 bu iki rolün izin matrisini ilk kez doldurduğu için geri alma = matrisi boşaltmak.
            migrationBuilder.Sql("""DELETE FROM "RolPermission" WHERE "RolId" IN (241341, 241342);""");
        }
    }
}
