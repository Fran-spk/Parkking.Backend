using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PoliticaPrimerPeriodoYQuitarUmbral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasUmbralProporcional",
                table: "Estacionamientos");

            migrationBuilder.AddColumn<int>(
                name: "PoliticaPrimerPeriodo",
                table: "Abonos",
                type: "integer",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PoliticaPrimerPeriodo",
                table: "Abonos");

            migrationBuilder.AddColumn<int>(
                name: "DiasUmbralProporcional",
                table: "Estacionamientos",
                type: "integer",
                nullable: true);
        }
    }
}
