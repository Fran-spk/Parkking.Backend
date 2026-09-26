using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AsignarCuentasCorrientesAMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuentaCorrienteClienteId",
                table: "Movimientos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaCorrienteEstacionamientoId",
                table: "Movimientos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_CuentaCorrienteClienteId",
                table: "Movimientos",
                column: "CuentaCorrienteClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_CuentaCorrienteEstacionamientoId",
                table: "Movimientos",
                column: "CuentaCorrienteEstacionamientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_CuentasCorrientesCliente_CuentaCorrienteCliente~",
                table: "Movimientos",
                column: "CuentaCorrienteClienteId",
                principalTable: "CuentasCorrientesCliente",
                principalColumn: "CuentaCorrienteClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_CuentasCorrientesEstacionamiento_CuentaCorrient~",
                table: "Movimientos",
                column: "CuentaCorrienteEstacionamientoId",
                principalTable: "CuentasCorrientesEstacionamiento",
                principalColumn: "CuentaCorrienteEstacionamientoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_CuentasCorrientesCliente_CuentaCorrienteCliente~",
                table: "Movimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_CuentasCorrientesEstacionamiento_CuentaCorrient~",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_CuentaCorrienteClienteId",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_CuentaCorrienteEstacionamientoId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "CuentaCorrienteClienteId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "CuentaCorrienteEstacionamientoId",
                table: "Movimientos");
        }
    }
}
