using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clases_KioPlus.Migrations
{
    /// <inheritdoc />
    public partial class AsociacionUnicaProductoProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductoProveedores_ProductoId",
                table: "ProductoProveedores");

            // Antes de exigir que la pareja sea única hay que resolver las que ya
            // estaban repetidas. Sobrevive la asociación más vieja, y se queda con
            // el precio más alto del grupo: es el mismo criterio que usa la compra,
            // donde un precio mayor se toma como aumento y uno menor como descuento.
            migrationBuilder.Sql(@"
                UPDATE pp
                SET PrecioCompra = g.PrecioMayor
                FROM ProductoProveedores pp
                INNER JOIN (
                    SELECT ProductoId, ProveedorId,
                           MIN(Id) AS IdQueQueda,
                           MAX(PrecioCompra) AS PrecioMayor
                    FROM ProductoProveedores
                    GROUP BY ProductoId, ProveedorId
                    HAVING COUNT(*) > 1
                ) g ON pp.Id = g.IdQueQueda;");

            migrationBuilder.Sql(@"
                DELETE pp
                FROM ProductoProveedores pp
                INNER JOIN (
                    SELECT ProductoId, ProveedorId, MIN(Id) AS IdQueQueda
                    FROM ProductoProveedores
                    GROUP BY ProductoId, ProveedorId
                    HAVING COUNT(*) > 1
                ) g ON pp.ProductoId = g.ProductoId
                   AND pp.ProveedorId = g.ProveedorId
                   AND pp.Id <> g.IdQueQueda;");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoProveedores_ProductoId_ProveedorId",
                table: "ProductoProveedores",
                columns: new[] { "ProductoId", "ProveedorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductoProveedores_ProductoId_ProveedorId",
                table: "ProductoProveedores");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoProveedores_ProductoId",
                table: "ProductoProveedores",
                column: "ProductoId");
        }
    }
}
