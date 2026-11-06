using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab3_proje.Migrations
{
    /// <inheritdoc />
    public partial class StajVeOgrenciTablolariEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BelgeTeslimYili",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DogumTarihi",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IkametgahAdresi",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgrenciNo",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OkuduguBolum",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OncekiStajGünSayisi",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaglikYardimStatüsü",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCKimlik",
                table: "Users",
                type: "INTEGER",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonNumarası",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uyruk",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StajBasvurulari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DosyaAdı = table.Column<string>(type: "TEXT", nullable: false),
                    DosyaYolu = table.Column<string>(type: "TEXT", nullable: false),
                    YuklemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Statu = table.Column<string>(type: "TEXT", nullable: false),
                    raporMesajı = table.Column<string>(type: "TEXT", nullable: true),
                    stajBaslangıcTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StajBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SecilenHaftalikGunler = table.Column<string>(type: "TEXT", nullable: false),
                    HesaplanmısStajGunSayısı = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StajBasvurulari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StajBasvurulari_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StajBasvurulari_UserId",
                table: "StajBasvurulari",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UserId",
                table: "Users",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UserId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "StajBasvurulari");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BelgeTeslimYili",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DogumTarihi",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IkametgahAdresi",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OgrenciNo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OkuduguBolum",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OncekiStajGünSayisi",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SaglikYardimStatüsü",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TCKimlik",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelefonNumarası",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Uyruk",
                table: "Users");
        }
    }
}
