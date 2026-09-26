using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReglaPorCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "ReglasAsignacion",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ReglasAsignacion" AS r
                SET "ClienteId" = a."ClienteId"
                FROM "Abonos" AS a
                WHERE r."Criterio" = 0
                  AND r."AbonoId" = a."AbonoId";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ReglasAsignacion_Abonos_AbonoId",
                table: "ReglasAsignacion");

            migrationBuilder.DropIndex(
                name: "IX_ReglasAsignacion_AbonoId",
                table: "ReglasAsignacion");

            migrationBuilder.DropColumn(
                name: "AbonoId",
                table: "ReglasAsignacion");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasAsignacion_ClienteId",
                table: "ReglasAsignacion",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReglasAsignacion_Clientes_ClienteId",
                table: "ReglasAsignacion",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReglasAsignacion_Clientes_ClienteId",
                table: "ReglasAsignacion");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "ReglasAsignacion",
                newName: "AbonoId");

            migrationBuilder.RenameIndex(
                name: "IX_ReglasAsignacion_ClienteId",
                table: "ReglasAsignacion",
                newName: "IX_ReglasAsignacion_AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReglasAsignacion_Abonos_AbonoId",
                table: "ReglasAsignacion",
                column: "AbonoId",
                principalTable: "Abonos",
                principalColumn: "AbonoId");
        }
    }
}
