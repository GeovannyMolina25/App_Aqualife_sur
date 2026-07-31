using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;

namespace rotter.Infraestructura.Data;

public class RotterDbContext : DbContext
{
    public RotterDbContext(DbContextOptions<RotterDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<IngresoCaja> IngresoCaja => Set<IngresoCaja>();
    public DbSet<DetalleIngresoCaja> DetalleIngresoCaja => Set<DetalleIngresoCaja>();
    public DbSet<SalidaCaja> SalidaCaja => Set<SalidaCaja>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Rol>(e => { e.ToTable("Roles"); e.Property(r => r.Nombre).HasMaxLength(50).IsRequired(); });

        m.Entity<Usuario>(e => {
            e.ToTable("Usuarios");
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            e.Property(u => u.Apellido).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500);
            e.Property(u => u.Sexo).HasMaxLength(20);
            e.Property(u => u.Direccion).HasMaxLength(300);
            e.HasOne(u => u.Rol).WithMany(r => r.Usuarios).HasForeignKey(u => u.RolId);
        });

        m.Entity<Categoria>(e => { e.ToTable("Categorias"); e.Property(c => c.Nombre).HasMaxLength(100).IsRequired(); });

        m.Entity<Producto>(e => {
            e.ToTable("Productos");
            e.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            e.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            e.Property(p => p.PrecioPromocion).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Categoria).WithMany(c => c.Productos).HasForeignKey(p => p.CategoriaId);
            e.HasOne(p => p.CreadoPor).WithMany().HasForeignKey(p => p.CreadoPorId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<Venta>(e => {
            e.ToTable("Ventas");
            e.Property(v => v.NumeroVenta).HasMaxLength(20).IsRequired();
            e.HasIndex(v => v.NumeroVenta).IsUnique();
            e.Property(v => v.Total).HasColumnType("decimal(18,2)");
            e.Property(v => v.Estado).HasMaxLength(50);
            e.HasOne(v => v.Cliente).WithMany(u => u.VentasComoCliente).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Colaborador).WithMany(u => u.VentasComoColaborador).HasForeignKey(v => v.ColaboradorId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<DetalleVenta>(e => {
            e.ToTable("DetalleVentas");
            e.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
            e.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
            e.HasOne(d => d.Venta).WithMany(v => v.Detalles).HasForeignKey(d => d.VentaId);
            e.HasOne(d => d.Producto).WithMany(p => p.Detalles).HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<Auditoria>(e => {
            e.ToTable("Auditoria");
            e.Property(a => a.Accion).HasMaxLength(100).IsRequired();
            e.Property(a => a.Entidad).HasMaxLength(100).IsRequired();
            e.Property(a => a.DatosAnteriores).HasColumnType("nvarchar(max)");
            e.Property(a => a.DatosNuevos).HasColumnType("nvarchar(max)");
        });

        m.Entity<IngresoCaja>(e => {
            e.ToTable("IngresoCaja");
            e.Property(i => i.NumeroIngreso).HasMaxLength(30).IsRequired();
            e.HasIndex(i => i.NumeroIngreso).IsUnique();
            e.Property(i => i.Banco).HasMaxLength(100).IsRequired();
            e.Property(i => i.NumeroTransaccion).HasMaxLength(100).IsRequired();
            e.Property(i => i.TotalIngresado).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Usuario).WithMany().HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<DetalleIngresoCaja>(e => {
            e.ToTable("DetalleIngresoCaja");
            e.HasOne(d => d.IngresoCaja).WithMany(i => i.Detalles).HasForeignKey(d => d.IngresoCajaId);
            e.HasOne(d => d.Venta).WithMany().HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Restrict);
        });

        m.Entity<SalidaCaja>(e => {
            e.ToTable("SalidaCaja");
            e.Property(s => s.NumeroSalida).HasMaxLength(30).IsRequired();
            e.HasIndex(s => s.NumeroSalida).IsUnique();
            e.Property(s => s.Valor).HasColumnType("decimal(18,2)");
            e.Property(s => s.Motivo).HasMaxLength(200).IsRequired();
            e.HasOne(s => s.Usuario).WithMany().HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
