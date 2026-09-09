using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MuhasibPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVersiyonlar",
                columns: table => new
                {
                    CurrentAppVersion = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentAppVersionLastUpdate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PreviousAppVersiyon = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersiyonlar", x => x.CurrentAppVersion);
                });

            migrationBuilder.CreateTable(
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaKodu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KisaUnvani = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TamUnvani = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    YetkiliKisi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Il = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PostaKodu = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    Telefon1 = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    Telefon2 = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    TCNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    Web = table.Column<string>(type: "TEXT", maxLength: 75, nullable: true),
                    Eposta = table.Column<string>(type: "TEXT", maxLength: 75, nullable: true),
                    Logo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LogoOnizleme = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PBu1 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PBu2 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalAyarlar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Anahtar = table.Column<string>(type: "TEXT", nullable: false),
                    Deger = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalAyarlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ParolaHash = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Adi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Soyadi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    Resim = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ResimOnizleme = table.Column<byte[]>(type: "BLOB", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRoller",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    RolAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    RolTip = table.Column<int>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciRoller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Tur = table.Column<int>(type: "INTEGER", nullable: false),
                    LisansAnahtari = table.Column<string>(type: "TEXT", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    User = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDbVersiyonlar",
                columns: table => new
                {
                    CurrentAppVersion = table.Column<string>(type: "TEXT", nullable: false),
                    DatabaseName = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentDatabaseVersion = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentDatabaseLastUpdate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PreviousDatabaseVersion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDbVersiyonlar", x => x.CurrentAppVersion);
                    table.ForeignKey(
                        name: "FK_AppDbVersiyonlar_AppVersiyonlar_CurrentAppVersion",
                        column: x => x.CurrentAppVersion,
                        principalTable: "AppVersiyonlar",
                        principalColumn: "CurrentAppVersion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaliDonemler",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliYil = table.Column<int>(type: "INTEGER", nullable: false),
                    DatabaseName = table.Column<string>(type: "TEXT", nullable: false),
                    DatabaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    DosyaBoyutu = table.Column<long>(type: "INTEGER", nullable: true),
                    SonYedekTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArsivlendiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaliDonemler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaliDonemler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLoglar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    IslemTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    HedefTablo = table.Column<string>(type: "TEXT", nullable: false),
                    HedefId = table.Column<string>(type: "TEXT", nullable: false),
                    EskiDeger = table.Column<string>(type: "TEXT", nullable: true),
                    YeniDeger = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    IpAdresi = table.Column<string>(type: "TEXT", nullable: true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLoglar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLoglar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hesaplar",
                columns: table => new
                {
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: true),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KullaniciId1 = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hesaplar", x => x.KullaniciId);
                    table.ForeignKey(
                        name: "FK_Hesaplar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hesaplar_Kullanicilar_KullaniciId1",
                        column: x => x.KullaniciId1,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OturumKayitlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MaliDonemId = table.Column<long>(type: "INTEGER", nullable: false),
                    OturumToken = table.Column<string>(type: "TEXT", nullable: false),
                    GirisZamani = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SonHeartbeat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CikisZamani = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IpAdresi = table.Column<string>(type: "TEXT", nullable: true),
                    KaydedenId = table.Column<long>(type: "INTEGER", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncelleyenId = table.Column<long>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArananTerim = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OturumKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OturumKayitlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OturumKayitlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciFirmaRol",
                columns: table => new
                {
                    KullaniciId = table.Column<long>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<long>(type: "INTEGER", nullable: false),
                    RolId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciFirmaRol", x => new { x.KullaniciId, x.FirmaId });
                    table.ForeignKey(
                        name: "FK_KullaniciFirmaRol_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciFirmaRol_KullaniciRoller_RolId",
                        column: x => x.RolId,
                        principalTable: "KullaniciRoller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KullaniciFirmaRol_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolPermission",
                columns: table => new
                {
                    RolId = table.Column<long>(type: "INTEGER", nullable: false),
                    PermissionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermission", x => new { x.RolId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolPermission_KullaniciRoller_RolId",
                        column: x => x.RolId,
                        principalTable: "KullaniciRoller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppVersiyonlar",
                columns: new[] { "CurrentAppVersion", "CurrentAppVersionLastUpdate", "PreviousAppVersiyon" },
                values: new object[] { "1.0.0", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.InsertData(
                table: "KullaniciRoller",
                columns: new[] { "Id", "Aciklama", "AktifMi", "ArananTerim", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "RolAdi", "RolTip" },
                values: new object[,]
                {
                    { 241341L, "Sistemi yönetme yetkisine sahip kullanıcı rolü", true, null, null, null, 5413300800L, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yönetici", 1 },
                    { 241342L, "Sistemi sınırlı şekilde kullanma yetkisine sahip kullanıcı rolü", true, null, null, null, 5413300800L, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kullanıcı", 2 }
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "Id", "Adi", "AktifMi", "ArananTerim", "Eposta", "GuncellemeTarihi", "GuncelleyenId", "KaydedenId", "KayitTarihi", "KullaniciAdi", "ParolaHash", "Resim", "ResimOnizleme", "Soyadi", "Telefon" },
                values: new object[] { 5413300800L, "Ömer", true, "korkutomer, Ömer Korkut, Yönetici", "korkutomer@gmail.com", null, null, 5413300800L, new DateTime(2025, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "korkutomer", "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==", null, null, "Korkut", "0 (541) 330 0800" });

            migrationBuilder.InsertData(
                table: "AppDbVersiyonlar",
                columns: new[] { "CurrentAppVersion", "CurrentDatabaseLastUpdate", "CurrentDatabaseVersion", "DatabaseName", "PreviousDatabaseVersion" },
                values: new object[] { "1.0.0", new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "1.0.0", "Sistem.db", null });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLoglar_KullaniciId",
                table: "AuditLoglar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Hesaplar_FirmaId",
                table: "Hesaplar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Hesaplar_KullaniciId1",
                table: "Hesaplar",
                column: "KullaniciId1");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciFirmaRol_FirmaId",
                table: "KullaniciFirmaRol",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciFirmaRol_RolId",
                table: "KullaniciFirmaRol",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_MaliDonemler_FirmaId",
                table: "MaliDonemler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_OturumKayitlari_FirmaId",
                table: "OturumKayitlari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_OturumKayitlari_KullaniciId",
                table: "OturumKayitlari",
                column: "KullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDbVersiyonlar");

            migrationBuilder.DropTable(
                name: "AuditLoglar");

            migrationBuilder.DropTable(
                name: "GlobalAyarlar");

            migrationBuilder.DropTable(
                name: "Hesaplar");

            migrationBuilder.DropTable(
                name: "KullaniciFirmaRol");

            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "MaliDonemler");

            migrationBuilder.DropTable(
                name: "OturumKayitlari");

            migrationBuilder.DropTable(
                name: "RolPermission");

            migrationBuilder.DropTable(
                name: "SistemLogs");

            migrationBuilder.DropTable(
                name: "AppVersiyonlar");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "KullaniciRoller");
        }
    }
}
