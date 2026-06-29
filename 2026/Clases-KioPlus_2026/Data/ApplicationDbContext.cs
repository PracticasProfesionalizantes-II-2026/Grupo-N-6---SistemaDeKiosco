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
}
