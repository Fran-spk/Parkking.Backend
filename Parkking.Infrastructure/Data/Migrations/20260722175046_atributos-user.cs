using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class atributosuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "USU_NOMBRE",
                table: "Usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "USU_TELEFONO",
                table: "Usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "USU_NOMBRE",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "USU_TELEFONO",
                table: "Usuarios");
        }
    }
}
