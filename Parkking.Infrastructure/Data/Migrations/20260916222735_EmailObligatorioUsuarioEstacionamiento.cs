using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmailObligatorioUsuarioEstacionamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Inventados para filas existentes sin mail (solo backend/seed).
            migrationBuilder.Sql("""
                UPDATE "Estacionamientos"
                SET "EmailAvisos" = 'avisos@parkking.local'
                WHERE "EmailAvisos" IS NULL OR btrim("EmailAvisos") = '';

                UPDATE "Usuarios"
                SET "Mail" = 'usuario' || "USU_ID"::text || '@parkking.local'
                WHERE "Mail" IS NULL OR btrim("Mail") = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "EmailAvisos",
                table: "Estacionamientos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmailAvisos",
                table: "Estacionamientos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
