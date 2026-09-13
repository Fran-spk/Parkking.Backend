using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecibos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recibos",
                columns: table => new
                {
                    ReciboId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    PagoId = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Recargo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClienteNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CocherasLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatentesLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PeriodosLabel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Cobrador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibos", x => x.ReciboId);
                    table.ForeignKey(
                        name: "FK_Recibos_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "PagoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_EstacionamientoId_Numero",
                table: "Recibos",
                columns: new[] { "EstacionamientoId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_PagoId",
                table: "Recibos",
                column: "PagoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recibos");
        }
    }
}
