using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Models
{
    public class Producto
    {
        public int Id { get; set; }
        [Required] public string Nombre { get; set; }
        [Required] public string Marca { get; set; }

        // Relación con la categoría a la que pertenece
        [Required] public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        // Precio de venta actual del producto
        [Required] public double PrecioVenta { get; set; }

        // Cantidad actual disponible en el stock
        [Required] public int StockDisponible { get; set; }

        // Días previos al vencimiento en que se debe generar una alerta
        [Required] public int DiasAvisoVencimiento { get; set; }

        // Relación uno a muchos con los lotes del producto
        public List<Lote> Lotes { get; set; }

        // Relación muchos a muchos con proveedores
        public List<ProductoProveedor> ProductoProveedores { get; set; }

        // Relación uno a muchos con los detalles de venta
        public List<DetalleVenta> DetallesVentas { get; set; }
        
    }
}
