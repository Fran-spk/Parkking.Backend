using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CargoAbono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbonoId",
                table: "Cargos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_AbonoId",
                table: "Cargos",
                column: "AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cargos_Abonos_AbonoId",
                table: "Cargos",
                column: "AbonoId",
                principalTable: "Abonos",
                principalColumn: "AbonoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cargos_Abonos_AbonoId",
                table: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Cargos_AbonoId",
                table: "Cargos");

            migrationBuilder.DropColumn(
                name: "AbonoId",
                table: "Cargos");
        }
    }
}
