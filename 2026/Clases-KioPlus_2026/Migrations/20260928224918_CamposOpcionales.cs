using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clases_KioPlus.Migrations
{
    /// <inheritdoc />
    public partial class CamposOpcionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaPago",
                table: "Ventas",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorreoElectronico",
                table: "CuentasCorrientesClientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Lo que antes era "vacío" disfrazado de dato ahora es NULL de verdad:
            // una venta sin cobrar guardaba la fecha 0001-01-01, y los campos de
            // texto opcionales guardaban cadena vacía.
            migrationBuilder.Sql(
                "UPDATE Ventas SET FechaPago = NULL WHERE FechaPago = '0001-01-01T00:00:00';");
            migrationBuilder.Sql(
                "UPDATE CuentasCorrientesClientes SET CorreoElectronico = NULL WHERE CorreoElectronico = '';");
            migrationBuilder.Sql(
                "UPDATE Proveedores SET Observaciones = NULL WHERE Observaciones = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaPago",
                table: "Ventas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Proveedores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CorreoElectronico",
                table: "CuentasCorrientesClientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
