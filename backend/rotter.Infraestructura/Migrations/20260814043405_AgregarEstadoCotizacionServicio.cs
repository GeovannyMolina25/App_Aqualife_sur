using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoCotizacionServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "CotizacionesServicio",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Entrante");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "CotizacionesServicio",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "CotizacionesServicio");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "CotizacionesServicio");
        }
    }
}
