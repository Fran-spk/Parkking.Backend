using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDetalleCuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetallesCuota",
                columns: table => new
                {
                    DetalleCuotaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    CuotaId = table.Column<int>(type: "integer", nullable: false),
                    TarifaMensualId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    VehiculoId = table.Column<int>(type: "integer", nullable: true),
                    CocheraId = table.Column<int>(type: "integer", nullable: true),
                    Patente = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CocheraNumero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoVehiculoNombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CategoriaNombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCuota", x => x.DetalleCuotaId);
                    table.ForeignKey(
                        name: "FK_DetallesCuota_Cocheras_CocheraId",
                        column: x => x.CocheraId,
                        principalTable: "Cocheras",
                        principalColumn: "CocheraId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesCuota_Cuotas_CuotaId",
                        column: x => x.CuotaId,
                        principalTable: "Cuotas",
                        principalColumn: "CuotaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCuota_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCuota_TarifasMensuales_TarifaMensualId",
                        column: x => x.TarifaMensualId,
                        principalTable: "TarifasMensuales",
                        principalColumn: "TarifaMensualId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesCuota_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "VehiculoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuota_CocheraId",
                table: "DetallesCuota",
                column: "CocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuota_CuotaId",
                table: "DetallesCuota",
                column: "CuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuota_EstacionamientoId",
                table: "DetallesCuota",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuota_TarifaMensualId",
                table: "DetallesCuota",
                column: "TarifaMensualId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCuota_VehiculoId",
                table: "DetallesCuota",
                column: "VehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesCuota");
        }
    }
}
