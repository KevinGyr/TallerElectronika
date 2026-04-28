using Microsoft.EntityFrameworkCore;
using TallerElectronika.Models;

namespace TallerElectronika.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.ClienteId);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(500);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
    {
        entity.HasKey(e => e.ProductoId);
        entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
        entity.Property(e => e.CodigoProducto).IsRequired().HasMaxLength(50);
        entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
    });

            // Configuración de Venta
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasKey(e => e.VentaId);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.Impuesto).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.Estado).HasMaxLength(50);

                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de VentaDetalle
            modelBuilder.Entity<VentaDetalle>(entity =>
            {
                entity.HasKey(e => e.VentaDetalleId);
                entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);

                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.VentaDetalles)
                    .HasForeignKey(e => e.VentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.VentaDetalles)
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de MovimientoInventario
            modelBuilder.Entity<MovimientoInventario>(entity =>
    {
        entity.HasKey(e => e.MovimientoId);
        entity.Property(e => e.TipoMovimiento).IsRequired();
        entity.Property(e => e.FechaMovimiento).IsRequired();
        
        // Relación con Producto
        entity.HasOne(e => e.Producto)
            .WithMany(p => p.MovimientosInventario)
            .HasForeignKey(e => e.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    });

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.UsuarioId);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Rol).HasMaxLength(50);
            });
        }
    }
}
