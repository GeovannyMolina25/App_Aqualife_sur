using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPromocionBienvenida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPremioBienvenida",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PremioBienvenida",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PremioBienvenidaEntregado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecargasParaSeptimo",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaPremioBienvenida",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PremioBienvenida",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PremioBienvenidaEntregado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecargasParaSeptimo",
                table: "Usuarios");
        }
    }
}
