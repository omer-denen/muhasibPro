using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasibPro.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddTenantIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FirmaId",
                table: "TenantDatabaseVersiyonlar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MaliDonemId",
                table: "TenantDatabaseVersiyonlar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OlusturanKullaniciId",
                table: "TenantDatabaseVersiyonlar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MakineId",
                table: "TenantDatabaseVersiyonlar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KurulumId",
                table: "TenantDatabaseVersiyonlar",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "TenantDatabaseVersiyonlar");

            migrationBuilder.DropColumn(
                name: "MaliDonemId",
                table: "TenantDatabaseVersiyonlar");

            migrationBuilder.DropColumn(
                name: "OlusturanKullaniciId",
                table: "TenantDatabaseVersiyonlar");

            migrationBuilder.DropColumn(
                name: "MakineId",
                table: "TenantDatabaseVersiyonlar");

            migrationBuilder.DropColumn(
                name: "KurulumId",
                table: "TenantDatabaseVersiyonlar");
        }
    }
}
