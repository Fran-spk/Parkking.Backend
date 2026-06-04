using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking_backend.Migrations
{
    /// <inheritdoc />
    public partial class movimientosv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Clientes_ClienteId",
                table: "MovimientosCaja");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "MovimientosCaja",
                newName: "AbonoCocheraId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_ClienteId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_AbonoCocheraId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_AbonoCocheras_AbonoCocheraId",
                table: "MovimientosCaja",
                column: "AbonoCocheraId",
                principalTable: "AbonoCocheras",
                principalColumn: "AbonoCocheraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_AbonoCocheras_AbonoCocheraId",
                table: "MovimientosCaja");

            migrationBuilder.RenameColumn(
                name: "AbonoCocheraId",
                table: "MovimientosCaja",
                newName: "ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_AbonoCocheraId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Clientes_ClienteId",
                table: "MovimientosCaja",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "ClienteId");
        }
    }
}
