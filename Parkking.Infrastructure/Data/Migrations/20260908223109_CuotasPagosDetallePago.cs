using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CuotasPagosDetallePago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_PagosMensuales_PagoMensualId",
                table: "MovimientosCaja");

            migrationBuilder.AddColumn<int>(
                name: "PeriodicidadCobro",
                table: "Abonos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cuotas",
                columns: table => new
                {
                    CuotaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    PeriodoInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodoFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuotas", x => x.CuotaId);
                    table.ForeignKey(
                        name: "FK_Cuotas_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    PagoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Recargo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MercadoPagoId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.PagoId);
                    table.ForeignKey(
                        name: "FK_Pagos_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPago",
                columns: table => new
                {
                    DetallePagoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    PagoId = table.Column<int>(type: "integer", nullable: false),
                    CuotaId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPago", x => x.DetallePagoId);
                    table.ForeignKey(
                        name: "FK_DetallesPago_Cuotas_CuotaId",
                        column: x => x.CuotaId,
                        principalTable: "Cuotas",
                        principalColumn: "CuotaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "PagoId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Histórico: cada PagoMensual → Cuota (mes) Pagada + Pago (mismo Id) + DetallePago
            migrationBuilder.Sql("""
                INSERT INTO "Cuotas" ("EstacionamientoId", "AbonoId", "PeriodoInicio", "PeriodoFin", "Monto", "Estado")
                SELECT
                    MIN(pm."EstacionamientoId"),
                    pm."AbonoId",
                    make_date(EXTRACT(YEAR FROM pm."Mes")::int, EXTRACT(MONTH FROM pm."Mes")::int, 1),
                    (date_trunc('month', pm."Mes"::timestamp) + INTERVAL '1 month' - INTERVAL '1 day')::date,
                    SUM(pm."Monto" + COALESCE(pm."Recargo", 0)),
                    2
                FROM "PagosMensuales" pm
                GROUP BY pm."AbonoId",
                         date_trunc('month', pm."Mes"::timestamp),
                         EXTRACT(YEAR FROM pm."Mes"),
                         EXTRACT(MONTH FROM pm."Mes");

                INSERT INTO "Pagos" ("PagoId", "EstacionamientoId", "AbonoId", "MontoTotal", "Recargo", "FechaHora", "Observacion", "MercadoPagoId")
                OVERRIDING SYSTEM VALUE
                SELECT
                    pm."PagoMensualId",
                    pm."EstacionamientoId",
                    pm."AbonoId",
                    pm."Monto" + COALESCE(pm."Recargo", 0),
                    pm."Recargo",
                    pm."FechaHoraCarga",
                    pm."Observacion",
                    pm."MercadoPagoId"
                FROM "PagosMensuales" pm;

                SELECT setval(pg_get_serial_sequence('"Pagos"', 'PagoId'), COALESCE((SELECT MAX("PagoId") FROM "Pagos"), 1));

                INSERT INTO "DetallesPago" ("EstacionamientoId", "PagoId", "CuotaId", "Monto")
                SELECT
                    pm."EstacionamientoId",
                    pm."PagoMensualId",
                    c."CuotaId",
                    pm."Monto" + COALESCE(pm."Recargo", 0)
                FROM "PagosMensuales" pm
                INNER JOIN "Cuotas" c
                    ON c."AbonoId" = pm."AbonoId"
                   AND c."PeriodoInicio" = make_date(EXTRACT(YEAR FROM pm."Mes")::int, EXTRACT(MONTH FROM pm."Mes")::int, 1);
                """);

            migrationBuilder.DropTable(
                name: "PagosMensuales");

            migrationBuilder.RenameColumn(
                name: "PagoMensualId",
                table: "MovimientosCaja",
                newName: "PagoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_PagoMensualId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuotas_AbonoId_PeriodoInicio",
                table: "Cuotas",
                columns: new[] { "AbonoId", "PeriodoInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPago_CuotaId",
                table: "DetallesPago",
                column: "CuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPago_PagoId",
                table: "DetallesPago",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_AbonoId",
                table: "Pagos",
                column: "AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja",
                column: "PagoId",
                principalTable: "Pagos",
                principalColumn: "PagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Pagos_PagoId",
                table: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "DetallesPago");

            migrationBuilder.DropTable(
                name: "Cuotas");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropColumn(
                name: "PeriodicidadCobro",
                table: "Abonos");

            migrationBuilder.RenameColumn(
                name: "PagoId",
                table: "MovimientosCaja",
                newName: "PagoMensualId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_PagoId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_PagoMensualId");

            migrationBuilder.CreateTable(
                name: "PagosMensuales",
                columns: table => new
                {
                    PagoMensualId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    FechaHoraCarga = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MercadoPagoId = table.Column<string>(type: "text", nullable: true),
                    Mes = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Recargo = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosMensuales", x => x.PagoMensualId);
                    table.ForeignKey(
                        name: "FK_PagosMensuales_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosMensuales_AbonoId",
                table: "PagosMensuales",
                column: "AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_PagosMensuales_PagoMensualId",
                table: "MovimientosCaja",
                column: "PagoMensualId",
                principalTable: "PagosMensuales",
                principalColumn: "PagoMensualId");
        }
    }
}
