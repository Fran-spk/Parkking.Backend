using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MaxOcupacionCochera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxOcupacion",
                table: "Cocheras",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Cocheras"
                SET "MaxOcupacion" = 2
                WHERE "MultipleOcupacion" = TRUE
                  AND ("MaxOcupacion" IS NULL OR "MaxOcupacion" < 2);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxOcupacion",
                table: "Cocheras");
        }
    }
}
