using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasibPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedHashDuzeltme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: 5413300800L,
                column: "ParolaHash",
                value: "AQAAAAIAAYagAAAAEPm/gfxm9YLZq6cmA6QUFfQZfChx8epMnb8PmvRVXPH/Eq3aYjvyXNvclwOM2HHmdg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kullanicilar",
                keyColumn: "Id",
                keyValue: 5413300800L,
                column: "ParolaHash",
                value: "AQAAAAIAAYagAAAAECnYdlrjFiWFJc+FGeGDmvR87uz20oU/Z0K4JE9ddoF2VUnmHw0idEFX8UPOb4cpzQ==");
        }
    }
}
