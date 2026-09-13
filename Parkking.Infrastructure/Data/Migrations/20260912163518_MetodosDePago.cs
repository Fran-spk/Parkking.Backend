using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MetodosDePago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetodoPagoLabel",
                table: "Recibos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetodoDePagoId",
                table: "Pagos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetodosDePago",
                columns: table => new
                {
                    MetodoDePagoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosDePago", x => x.MetodoDePagoId);
                    table.ForeignKey(
                        name: "FK_MetodosDePago_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_MetodoDePagoId",
                table: "Pagos",
                column: "MetodoDePagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MetodosDePago_EstacionamientoId_Nombre",
                table: "MetodosDePago",
                columns: new[] { "EstacionamientoId", "Nombre" });

            migrationBuilder.Sql("""
                INSERT INTO "MetodosDePago" ("EstacionamientoId", "Nombre", "Activo")
                SELECT e."EstacionamientoId", v."Nombre", TRUE
                FROM "Estacionamientos" e
                CROSS JOIN (VALUES ('Efectivo'), ('Transferencia'), ('Débito')) AS v("Nombre")
                WHERE NOT EXISTS (
                    SELECT 1 FROM "MetodosDePago" m
                    WHERE m."EstacionamientoId" = e."EstacionamientoId"
                      AND lower(m."Nombre") = lower(v."Nombre")
                );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_MetodosDePago_MetodoDePagoId",
                table: "Pagos",
                column: "MetodoDePagoId",
                principalTable: "MetodosDePago",
                principalColumn: "MetodoDePagoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_MetodosDePago_MetodoDePagoId",
                table: "Pagos");

            migrationBuilder.DropTable(
                name: "MetodosDePago");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_MetodoDePagoId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MetodoPagoLabel",
                table: "Recibos");

            migrationBuilder.DropColumn(
                name: "MetodoDePagoId",
                table: "Pagos");
        }
    }
}
