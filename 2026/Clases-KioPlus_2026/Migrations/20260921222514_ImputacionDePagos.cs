using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clases_KioPlus.Migrations
{
    /// <inheritdoc />
    public partial class ImputacionDePagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MontoPagado",
                table: "Ventas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SaldoInicial",
                table: "CuentasCorrientesClientes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            // Toda venta ya pagada estaba cobrada por su total (Estado 0 = Pagado)
            migrationBuilder.Sql("UPDATE Ventas SET MontoPagado = MontoTotal WHERE Estado = 0;");

            // El saldo inicial es la parte de la deuda que no viene de ninguna venta:
            // lo que el cliente debía menos lo pendiente de sus ventas impagas.
            // (Estado 1 = NoPagado, FormaPago 0 = CuentaCorriente)
            migrationBuilder.Sql(@"
                UPDATE c
                SET SaldoInicial = CASE
                        WHEN c.MontoAdeudado - ISNULL(v.Pendiente, 0) > 0
                        THEN c.MontoAdeudado - ISNULL(v.Pendiente, 0)
                        ELSE 0
                    END
                FROM CuentasCorrientesClientes c
                LEFT JOIN (
                    SELECT CuentaCorrienteClienteId, SUM(MontoTotal - MontoPagado) AS Pendiente
                    FROM Ventas
                    WHERE Finalizada = 1 AND FormaPago = 0 AND Estado = 1
                    GROUP BY CuentaCorrienteClienteId
                ) v ON v.CuentaCorrienteClienteId = c.Id;");

            // A partir de acá el monto adeudado es siempre saldo inicial + ventas impagas
            migrationBuilder.Sql(@"
                UPDATE c
                SET MontoAdeudado = c.SaldoInicial + ISNULL(v.Pendiente, 0),
                    Estado = CASE WHEN c.SaldoInicial + ISNULL(v.Pendiente, 0) > 0 THEN 0 ELSE 1 END
                FROM CuentasCorrientesClientes c
                LEFT JOIN (
                    SELECT CuentaCorrienteClienteId, SUM(MontoTotal - MontoPagado) AS Pendiente
                    FROM Ventas
                    WHERE Finalizada = 1 AND FormaPago = 0 AND Estado = 1
                    GROUP BY CuentaCorrienteClienteId
                ) v ON v.CuentaCorrienteClienteId = c.Id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoPagado",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "SaldoInicial",
                table: "CuentasCorrientesClientes");
        }
    }
}
