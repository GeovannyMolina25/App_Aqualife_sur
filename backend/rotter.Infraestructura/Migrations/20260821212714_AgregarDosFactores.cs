using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDosFactores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorHabilitado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecretCifrado",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorHabilitado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecretCifrado",
                table: "Usuarios");
        }
    }
}
