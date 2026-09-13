using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v2propusu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "USU_USUARIO",
                table: "Usuarios",
                newName: "UsuarioName");

            migrationBuilder.RenameColumn(
                name: "USU_TELEFONO",
                table: "Usuarios",
                newName: "Telefono");

            migrationBuilder.RenameColumn(
                name: "USU_NOMBRE",
                table: "Usuarios",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "USU_MAIL",
                table: "Usuarios",
                newName: "Mail");

            migrationBuilder.RenameColumn(
                name: "USU_CLAVE",
                table: "Usuarios",
                newName: "Clave");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsuarioName",
                table: "Usuarios",
                newName: "USU_USUARIO");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "Usuarios",
                newName: "USU_TELEFONO");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Usuarios",
                newName: "USU_NOMBRE");

            migrationBuilder.RenameColumn(
                name: "Mail",
                table: "Usuarios",
                newName: "USU_MAIL");

            migrationBuilder.RenameColumn(
                name: "Clave",
                table: "Usuarios",
                newName: "USU_CLAVE");
        }
    }
}
