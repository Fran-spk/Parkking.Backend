using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovimientoAbonoYTipoGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbonoId",
                table: "Movimientos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoGastoId",
                table: "Movimientos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_AbonoId",
                table: "Movimientos",
                column: "AbonoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_TipoGastoId",
                table: "Movimientos",
                column: "TipoGastoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Abonos_AbonoId",
                table: "Movimientos",
                column: "AbonoId",
                principalTable: "Abonos",
                principalColumn: "AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_TiposGasto_TipoGastoId",
                table: "Movimientos",
                column: "TipoGastoId",
                principalTable: "TiposGasto",
                principalColumn: "TipoGastoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Abonos_AbonoId",
                table: "Movimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_TiposGasto_TipoGastoId",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_AbonoId",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_TipoGastoId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "AbonoId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "TipoGastoId",
                table: "Movimientos");
        }
    }
}
