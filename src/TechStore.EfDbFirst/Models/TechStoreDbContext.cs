using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TechStore.EfDbFirst.Models;

public partial class TechStoreDbContext : DbContext
{
    public TechStoreDbContext(DbContextOptions<TechStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVentas { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(e => e.Nombre, "UQ_Categorias_Nombre").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Clientes_Email").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.DetalleId);

            entity.HasIndex(e => e.ProductoId, "IX_DetalleVentas_ProductoId");

            entity.HasIndex(e => e.VentaId, "IX_DetalleVentas_VentaId");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentas_Productos");

            entity.HasOne(d => d.Venta).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVentas_Ventas");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasIndex(e => e.CategoriaId, "IX_Productos_CategoriaId");

            entity.HasIndex(e => e.Nombre, "IX_Productos_Nombre");

            entity.HasIndex(e => e.Sku, "UQ_Productos_Sku").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Productos_Activo");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())", "DF_Productos_FechaCreacion");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Sku).HasMaxLength(30);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productos_Categorias");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasIndex(e => e.ClienteId, "IX_Ventas_ClienteId");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Ventas_Fecha");
            entity.Property(e => e.Total).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Venta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Clientes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
