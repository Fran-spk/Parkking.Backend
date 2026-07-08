using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class userkfmov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MovimientosCaja",
                newName: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "USU_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCaja_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "MovimientosCaja",
                newName: "UserId");
        }
    }
}
