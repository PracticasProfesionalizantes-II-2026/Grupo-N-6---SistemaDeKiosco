using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clases_KioPlus.Migrations
{
    /// <inheritdoc />
    public partial class FechaAltaCuentaCorriente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "CuentasCorrientesClientes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            // Las cuentas que ya existían no tienen registrada su fecha de alta. Se
            // toma la de su primera venta; las que no tienen ventas quedan con la
            // fecha en que se aplica esta migración.
            migrationBuilder.Sql(@"
                UPDATE c
                SET FechaAlta = v.PrimeraVenta
                FROM CuentasCorrientesClientes c
                JOIN (SELECT CuentaCorrienteClienteId, MIN(FechaHora) AS PrimeraVenta
                      FROM Ventas
                      GROUP BY CuentaCorrienteClienteId) v
                  ON v.CuentaCorrienteClienteId = c.Id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "CuentasCorrientesClientes");
        }
    }
}
