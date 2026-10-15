using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVentas => Set<DetalleVenta>();
    public DbSet<CuentaCorrienteCliente> CuentasCorrientesClientes => Set<CuentaCorrienteCliente>();
    public DbSet<ProductoProveedor> ProductoProveedores => Set<ProductoProveedor>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<CompraProveedor> Compras => Set<CompraProveedor>();
    public DbSet<DetalleCompra> DetallesCompras => Set<DetalleCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Un producto no puede estar asociado dos veces al mismo proveedor: si
        // cambia el precio se edita la asociación que ya existe, no se crea otra.
        modelBuilder.Entity<ProductoProveedor>()
            .HasIndex(pp => new { pp.ProductoId, pp.ProveedorId })
            .IsUnique();

        // Los datos maestros (usuarios, clientes, productos, categorías y proveedores)
        // se dan de baja de forma lógica: su historial no puede borrarse en cascada.
        // Solo los renglones siguen cayendo junto con su venta o compra al anularla.
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Usuario).WithMany(u => u.Ventas)
            .HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.CuentaCorrienteCliente).WithMany(c => c.Ventas)
            .HasForeignKey(v => v.CuentaCorrienteClienteId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto).WithMany(p => p.DetallesVentas)
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Producto).WithMany()
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Producto).WithMany(p => p.Lotes)
            .HasForeignKey(l => l.ProductoId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoProveedor>()
            .HasOne(pp => pp.Producto).WithMany(p => p.ProductoProveedores)
            .HasForeignKey(pp => pp.ProductoId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductoProveedor>()
            .HasOne(pp => pp.Proveedor).WithMany(p => p.ProductoProveedores)
            .HasForeignKey(pp => pp.ProveedorId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria).WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CompraProveedor>()
            .HasOne(c => c.Proveedor).WithMany()
            .HasForeignKey(c => c.ProveedorId).OnDelete(DeleteBehavior.Restrict);
    }
}
