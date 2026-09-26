using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTarifasTodasPeriodicidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "TarifasMensuales"
                    ("EstacionamientoId", "TipoVehiculoId", "CategoriaCocheraId", "PeriodicidadCobro", "Precio", "FechaHoraActualizacion")
                SELECT
                    c."EstacionamientoId",
                    c."TipoVehiculoId",
                    c."CategoriaCocheraId",
                    p.per,
                    GREATEST(1, ROUND(c.precio_mensual * p.factor)),
                    NOW() AT TIME ZONE 'UTC'
                FROM (
                    SELECT
                        e."EstacionamientoId",
                        tv."TipoVehiculoId",
                        cc."CategoriaCocheraId",
                        COALESCE((
                            SELECT t."Precio"
                            FROM "TarifasMensuales" t
                            WHERE t."EstacionamientoId" = e."EstacionamientoId"
                              AND t."TipoVehiculoId" = tv."TipoVehiculoId"
                              AND t."CategoriaCocheraId" = cc."CategoriaCocheraId"
                              AND t."PeriodicidadCobro" = 0
                            ORDER BY t."FechaHoraActualizacion" DESC
                            LIMIT 1
                        ), 50000) AS precio_mensual
                    FROM "Estacionamientos" e
                    INNER JOIN "TiposVehiculo" tv
                        ON tv."EstacionamientoId" = e."EstacionamientoId" AND tv."Activo" = TRUE
                    INNER JOIN "CategoriasCochera" cc
                        ON cc."EstacionamientoId" = e."EstacionamientoId" AND cc."Activo" = TRUE
                ) c
                CROSS JOIN (
                    VALUES
                        (0, 1.0),
                        (1, 0.5),
                        (2, 2.0),
                        (3, 3.0),
                        (4, 6.0),
                        (5, 12.0)
                ) AS p(per, factor)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "TarifasMensuales" x
                    WHERE x."EstacionamientoId" = c."EstacionamientoId"
                      AND x."TipoVehiculoId" = c."TipoVehiculoId"
                      AND x."CategoriaCocheraId" = c."CategoriaCocheraId"
                      AND x."PeriodicidadCobro" = p.per
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
