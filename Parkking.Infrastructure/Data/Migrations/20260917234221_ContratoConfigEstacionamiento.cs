using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContratoConfigEstacionamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContratoPlazoMeses",
                table: "Estacionamientos",
                type: "integer",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<bool>(
                name: "ContratoSeguroObligatorio",
                table: "Estacionamientos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                """
                UPDATE "Estacionamientos"
                SET "ContratoPlazoMeses" = 12
                WHERE "ContratoPlazoMeses" < 1;

                UPDATE "Estacionamientos"
                SET "ContratoSeguroObligatorio" = TRUE
                WHERE TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContratoPlazoMeses",
                table: "Estacionamientos");

            migrationBuilder.DropColumn(
                name: "ContratoSeguroObligatorio",
                table: "Estacionamientos");
        }
    }
}
