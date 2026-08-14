using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rotter.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCategoriaTipoYCotizacionesServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Categorias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Producto");

            migrationBuilder.CreateTable(
                name: "CotizacionesServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    NombreContacto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TamanoEspacio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaDeseada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionesServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionesServicio_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesServicio_Usuarios_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesServicio_ClienteId",
                table: "CotizacionesServicio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesServicio_ProductoId",
                table: "CotizacionesServicio",
                column: "ProductoId");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = 'Servicios de Limpieza')
                    INSERT INTO Categorias (Nombre, Descripcion, Tipo) VALUES
                    ('Servicios de Limpieza', 'Servicios de limpieza a domicilio', 'Servicio');
            ");

            migrationBuilder.Sql(@"
                DECLARE @CategoriaServiciosId INT = (SELECT Id FROM Categorias WHERE Nombre = 'Servicios de Limpieza');
                DECLARE @CreadoPorId INT = (SELECT TOP 1 Id FROM Usuarios WHERE Email = 'pedidos-web@acqualife.com');
                IF @CreadoPorId IS NULL SET @CreadoPorId = (SELECT TOP 1 Id FROM Usuarios ORDER BY Id);

                IF @CategoriaServiciosId IS NOT NULL AND @CreadoPorId IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM Productos WHERE Nombre = 'Limpieza de casa/apartamento')
                        INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, CategoriaId, EsPromocion, Activo, CreadoPorId, FechaCreacion)
                        VALUES ('Limpieza de casa/apartamento', 'Limpieza profunda a domicilio para tu hogar o apartamento.', 0, 0, @CategoriaServiciosId, 0, 1, @CreadoPorId, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM Productos WHERE Nombre = 'Limpieza de oficina')
                        INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, CategoriaId, EsPromocion, Activo, CreadoPorId, FechaCreacion)
                        VALUES ('Limpieza de oficina', 'Limpieza y mantenimiento de espacios de oficina.', 0, 0, @CategoriaServiciosId, 0, 1, @CreadoPorId, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM Productos WHERE Nombre = 'Limpieza por días')
                        INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, CategoriaId, EsPromocion, Activo, CreadoPorId, FechaCreacion)
                        VALUES ('Limpieza por días', 'Servicio de limpieza recurrente contratado por días.', 0, 0, @CategoriaServiciosId, 0, 1, @CreadoPorId, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM Productos WHERE Nombre = 'Limpieza de carro/vehículo')
                        INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, CategoriaId, EsPromocion, Activo, CreadoPorId, FechaCreacion)
                        VALUES ('Limpieza de carro/vehículo', 'Limpieza interior y exterior de tu vehículo a domicilio.', 0, 0, @CategoriaServiciosId, 0, 1, @CreadoPorId, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM Productos WHERE Nombre = 'Limpieza y desinfección de cuartos hospitalarios')
                        INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, CategoriaId, EsPromocion, Activo, CreadoPorId, FechaCreacion)
                        VALUES ('Limpieza y desinfección de cuartos hospitalarios', 'Limpieza y desinfección especializada de cuartos hospitalarios.', 0, 0, @CategoriaServiciosId, 0, 1, @CreadoPorId, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionesServicio");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Categorias");
        }
    }
}
