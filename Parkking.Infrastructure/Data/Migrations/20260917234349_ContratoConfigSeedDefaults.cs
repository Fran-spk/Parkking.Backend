using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContratoConfigSeedDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Estacionamientos"
                SET "ContratoPlazoMeses" = 12
                WHERE "ContratoPlazoMeses" < 1;

                UPDATE "Estacionamientos"
                SET "ContratoSeguroObligatorio" = TRUE
                WHERE "ContratoSeguroObligatorio" = FALSE
                  AND "ContratoPlazoMeses" = 12;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
