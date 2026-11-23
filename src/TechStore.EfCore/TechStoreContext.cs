using Microsoft.EntityFrameworkCore;
using TechStore.Core;

namespace TechStore.EfCore;

/// <summary>
/// Modelo Code First. Los nombres de tablas, columnas, restricciones e índices están configurados
/// para coincidir con TechStoreDB.sql, así las migraciones generan el mismo esquema.
/// </summary>
public class TechStoreContext : DbContext
{
    public TechStoreContext(DbContextOptions<TechStoreContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

    public static TechStoreContext Crear(string connectionString)
    {
        var opciones = new DbContextOptionsBuilder<TechStoreContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new TechStoreContext(opciones);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.HasKey(x => x.CategoriaId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UQ_Categorias_Nombre");
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos", t =>
            {
                t.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
                t.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
            });
            e.HasKey(x => x.ProductoId);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Sku).HasMaxLength(30).IsRequired();
            e.Property(x => x.Precio).HasPrecision(10, 2);
            e.Property(x => x.FechaCreacion).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Sku).IsUnique().HasDatabaseName("UQ_Productos_Sku");
            e.HasIndex(x => x.Nombre).HasDatabaseName("IX_Productos_Nombre");
            e.HasIndex(x => x.CategoriaId).HasDatabaseName("IX_Productos_CategoriaId");
            e.HasOne<Categoria>().WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(x => x.ClienteId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Clientes_Email");
        });

        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Ventas", t => t.HasCheckConstraint("CK_Ventas_Total", "[Total] >= 0"));
            e.HasKey(x => x.VentaId);
            e.Property(x => x.Fecha).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.Total).HasPrecision(12, 2);
            e.HasIndex(x => x.ClienteId).HasDatabaseName("IX_Ventas_ClienteId");
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Detalles).WithOne().HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetalleVentas", t =>
            {
                t.HasCheckConstraint("CK_DetalleVentas_Cantidad", "[Cantidad] > 0");
                t.HasCheckConstraint("CK_DetalleVentas_Precio", "[PrecioUnitario] > 0");
            });
            e.HasKey(x => x.DetalleId);
            e.Property(x => x.PrecioUnitario).HasPrecision(10, 2);
            e.HasIndex(x => x.VentaId).HasDatabaseName("IX_DetalleVentas_VentaId");
            e.HasIndex(x => x.ProductoId).HasDatabaseName("IX_DetalleVentas_ProductoId");
            e.HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
