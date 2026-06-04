using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovePer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccionUsuario_Personas_UsuariosPER_ID",
                table: "AccionUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuario_Personas_UsuariosPER_ID",
                table: "GrupoUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Estado_Usuario_EST_USU_ID",
                table: "Personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Personas",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "PER_NOMBRE",
                table: "Personas");

            migrationBuilder.RenameTable(
                name: "Personas",
                newName: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "UsuariosPER_ID",
                table: "GrupoUsuario",
                newName: "UsuariosUSU_ID");

            migrationBuilder.RenameIndex(
                name: "IX_GrupoUsuario_UsuariosPER_ID",
                table: "GrupoUsuario",
                newName: "IX_GrupoUsuario_UsuariosUSU_ID");

            migrationBuilder.RenameColumn(
                name: "UsuariosPER_ID",
                table: "AccionUsuario",
                newName: "UsuariosUSU_ID");

            migrationBuilder.RenameIndex(
                name: "IX_AccionUsuario_UsuariosPER_ID",
                table: "AccionUsuario",
                newName: "IX_AccionUsuario_UsuariosUSU_ID");

            migrationBuilder.RenameColumn(
                name: "PER_ID",
                table: "Usuarios",
                newName: "USU_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Personas_EST_USU_ID",
                table: "Usuarios",
                newName: "IX_Usuarios_EST_USU_ID");

            migrationBuilder.AlterColumn<string>(
                name: "USU_USUARIO",
                table: "Usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "USU_MAIL",
                table: "Usuarios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "USU_CLAVE",
                table: "Usuarios",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "USU_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AccionUsuario_Usuarios_UsuariosUSU_ID",
                table: "AccionUsuario",
                column: "UsuariosUSU_ID",
                principalTable: "Usuarios",
                principalColumn: "USU_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuario_Usuarios_UsuariosUSU_ID",
                table: "GrupoUsuario",
                column: "UsuariosUSU_ID",
                principalTable: "Usuarios",
                principalColumn: "USU_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Estado_Usuario_EST_USU_ID",
                table: "Usuarios",
                column: "EST_USU_ID",
                principalTable: "Estado_Usuario",
                principalColumn: "EST_USU_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccionUsuario_Usuarios_UsuariosUSU_ID",
                table: "AccionUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuario_Usuarios_UsuariosUSU_ID",
                table: "GrupoUsuario");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Estado_Usuario_EST_USU_ID",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Personas");

            migrationBuilder.RenameColumn(
                name: "UsuariosUSU_ID",
                table: "GrupoUsuario",
                newName: "UsuariosPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_GrupoUsuario_UsuariosUSU_ID",
                table: "GrupoUsuario",
                newName: "IX_GrupoUsuario_UsuariosPER_ID");

            migrationBuilder.RenameColumn(
                name: "UsuariosUSU_ID",
                table: "AccionUsuario",
                newName: "UsuariosPER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_AccionUsuario_UsuariosUSU_ID",
                table: "AccionUsuario",
                newName: "IX_AccionUsuario_UsuariosPER_ID");

            migrationBuilder.RenameColumn(
                name: "USU_ID",
                table: "Personas",
                newName: "PER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_EST_USU_ID",
                table: "Personas",
                newName: "IX_Personas_EST_USU_ID");

            migrationBuilder.AlterColumn<string>(
                name: "USU_USUARIO",
                table: "Personas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "USU_MAIL",
                table: "Personas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "USU_CLAVE",
                table: "Personas",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Personas",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PER_NOMBRE",
                table: "Personas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Personas",
                table: "Personas",
                column: "PER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AccionUsuario_Personas_UsuariosPER_ID",
                table: "AccionUsuario",
                column: "UsuariosPER_ID",
                principalTable: "Personas",
                principalColumn: "PER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuario_Personas_UsuariosPER_ID",
                table: "GrupoUsuario",
                column: "UsuariosPER_ID",
                principalTable: "Personas",
                principalColumn: "PER_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Estado_Usuario_EST_USU_ID",
                table: "Personas",
                column: "EST_USU_ID",
                principalTable: "Estado_Usuario",
                principalColumn: "EST_USU_ID");
        }
    }
}
