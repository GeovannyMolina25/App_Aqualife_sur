using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeguridadUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpira",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereCambioPassword",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpira",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RequiereCambioPassword",
                table: "Usuarios");
        }
    }
}
