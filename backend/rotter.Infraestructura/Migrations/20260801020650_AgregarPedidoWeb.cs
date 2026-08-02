using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPedidoWeb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComprobanteUrl",
                table: "Ventas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Ventas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Impuestos",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origen",
                table: "Ventas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Presencial");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "IngresoCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroIngreso = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Banco = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroTransaccion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ComprobanteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalIngresado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresoCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngresoCaja_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalidaCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSalida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Banco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroTransaccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComprobanteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidaCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalidaCaja_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleIngresoCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngresoCajaId = table.Column<int>(type: "int", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleIngresoCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleIngresoCaja_IngresoCaja_IngresoCajaId",
                        column: x => x.IngresoCajaId,
                        principalTable: "IngresoCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleIngresoCaja_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleIngresoCaja_IngresoCajaId",
                table: "DetalleIngresoCaja",
                column: "IngresoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleIngresoCaja_VentaId",
                table: "DetalleIngresoCaja",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresoCaja_NumeroIngreso",
                table: "IngresoCaja",
                column: "NumeroIngreso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngresoCaja_UsuarioId",
                table: "IngresoCaja",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SalidaCaja_NumeroSalida",
                table: "SalidaCaja",
                column: "NumeroSalida",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalidaCaja_UsuarioId",
                table: "SalidaCaja",
                column: "UsuarioId");

            // Usuario "sistema" (Activo=false => no puede loguearse, LoginHandler lo rechaza)
            // usado como ColaboradorId de los pedidos autoservicio del catálogo público.
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Nombre", "Apellido", "Email", "PasswordHash", "FechaNacimiento", "Sexo", "Direccion", "Telefono", "RolId", "Activo", "FechaCreacion" },
                values: new object[] { "Pedidos", "Web", "pedidos-web@acqualife.com", BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), new DateOnly(2000, 1, 1), "Otro", "N/A", null, 2, false, DateTime.Now });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Email",
                keyValue: "pedidos-web@acqualife.com");

            migrationBuilder.DropTable(
                name: "DetalleIngresoCaja");

            migrationBuilder.DropTable(
                name: "SalidaCaja");

            migrationBuilder.DropTable(
                name: "IngresoCaja");

            migrationBuilder.DropColumn(
                name: "ComprobanteUrl",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Impuestos",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Ventas");
        }
    }
}
