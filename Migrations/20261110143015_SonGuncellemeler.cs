using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab3_proje.Migrations
{
    /// <inheritdoc />
    public partial class SonGuncellemeler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TCKimlik",
                table: "Users",
                type: "TEXT",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldMaxLength: 11,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TCKimlik",
                table: "Users",
                type: "INTEGER",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 11,
                oldNullable: true);
        }
    }
}
