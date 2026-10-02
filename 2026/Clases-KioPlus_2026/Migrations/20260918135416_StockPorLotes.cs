using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clases_KioPlus.Migrations
{
    /// <inheritdoc />
    public partial class StockPorLotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Finalizada",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Lotes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CantidadInicial",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DetalleCompraId",
                table: "Lotes",
                type: "int",
                nullable: true);

            // Los lotes que ya existían ingresaron con la cantidad que tienen hoy
            migrationBuilder.Sql("UPDATE Lotes SET CantidadInicial = Cantidad;");

            // Toda venta registrada antes de este cambio ya estaba cerrada
            migrationBuilder.Sql("UPDATE Ventas SET Finalizada = 1;");

            // El stock que había entrado por compras no tenía lote: se le arma uno
            // de apertura, sin vencimiento, para no perder la mercadería existente.
            migrationBuilder.Sql(@"
                INSERT INTO Lotes (NroLote, FechaVencimiento, CantidadInicial, Cantidad, ProductoId, DetalleCompraId)
                SELECT 'APERTURA-' + CAST(p.Id AS varchar(10)), NULL, p.StockDisponible, p.StockDisponible, p.Id, NULL
                FROM Productos p
                WHERE p.StockDisponible > 0
                  AND NOT EXISTS (SELECT 1 FROM Lotes l WHERE l.ProductoId = p.Id);");

            // A partir de acá el stock del producto es siempre la suma de sus lotes
            migrationBuilder.Sql(@"
                UPDATE Productos
                SET StockDisponible = ISNULL((SELECT SUM(l.Cantidad) FROM Lotes l WHERE l.ProductoId = Productos.Id), 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Finalizada",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CantidadInicial",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "DetalleCompraId",
                table: "Lotes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Lotes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
