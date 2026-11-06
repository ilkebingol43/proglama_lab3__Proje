using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab3_proje.Migrations
{
    /// <inheritdoc />
    public partial class yeniprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "stajBaslangıcTarihi",
                table: "StajBasvurulari",
                newName: "StajBaslangicTarihi");

            migrationBuilder.RenameColumn(
                name: "DosyaAdı",
                table: "StajBasvurulari",
                newName: "DosyaAdi");

            migrationBuilder.AddColumn<string>(
                name: "DosyaUzantisi",
                table: "StajBasvurulari",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosyaUzantisi",
                table: "StajBasvurulari");

            migrationBuilder.RenameColumn(
                name: "StajBaslangicTarihi",
                table: "StajBasvurulari",
                newName: "stajBaslangıcTarihi");

            migrationBuilder.RenameColumn(
                name: "DosyaAdi",
                table: "StajBasvurulari",
                newName: "DosyaAdı");
        }
    }
}
