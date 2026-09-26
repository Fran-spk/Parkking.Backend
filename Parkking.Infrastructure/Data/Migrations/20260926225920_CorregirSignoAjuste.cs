using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorregirSignoAjuste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "CuentasCorrientesEstacionamiento" AS c
                SET "Saldo" = c."Saldo" - 2 * x.suma
                FROM (
                    SELECT "CuentaCorrienteEstacionamientoId", SUM("Importe") AS suma
                    FROM "Movimientos"
                    WHERE "Tipo" = 3
                    GROUP BY "CuentaCorrienteEstacionamientoId"
                ) AS x
                WHERE c."CuentaCorrienteEstacionamientoId" = x."CuentaCorrienteEstacionamientoId";

                UPDATE "Movimientos"
                SET "Importe" = -"Importe"
                WHERE "Tipo" = 3;

                UPDATE "AjustesFinancieros"
                SET "Importe" = -"Importe";
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Movimientos"
                SET "Importe" = -"Importe"
                WHERE "Tipo" = 3;

                UPDATE "AjustesFinancieros"
                SET "Importe" = -"Importe";

                UPDATE "CuentasCorrientesEstacionamiento" AS c
                SET "Saldo" = c."Saldo" + 2 * x.suma
                FROM (
                    SELECT "CuentaCorrienteEstacionamientoId", SUM("Importe") AS suma
                    FROM "Movimientos"
                    WHERE "Tipo" = 3
                    GROUP BY "CuentaCorrienteEstacionamientoId"
                ) AS x
                WHERE c."CuentaCorrienteEstacionamientoId" = x."CuentaCorrienteEstacionamientoId";
                """);
        }
    }
}
