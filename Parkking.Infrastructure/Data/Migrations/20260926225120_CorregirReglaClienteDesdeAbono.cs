using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorregirReglaClienteDesdeAbono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "ReglasAsignacion" AS r
                SET "ClienteId" = a."ClienteId"
                FROM "Abonos" AS a
                WHERE r."Criterio" = 0
                  AND r."ClienteId" = a."AbonoId"
                  AND r."ClienteId" IS DISTINCT FROM a."ClienteId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
