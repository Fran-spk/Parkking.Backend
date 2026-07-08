using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class segv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccionUsuarioEstacionamiento_UsuarioEstacionamientos_Usuari~",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuarioEstacionamiento_UsuarioEstacionamientos_Usuario~",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioEstacionamientos_Estacionamientos_EstacionamientoId",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioEstacionamientos_EstacionamientoId",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrupoUsuarioEstacionamiento",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_U~",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_Cocheras_EstacionamientoId",
                table: "Cocheras");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccionUsuarioEstacionamiento",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_~",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropColumn(
                name: "EstacionamientoId",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropColumn(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropColumn(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.RenameColumn(
                name: "Usuario_EstacionamientoESTACIONAMIENTO_ID",
                table: "GrupoUsuarioEstacionamiento",
                newName: "Usuario_EstacionamientoUsuarioEstacionamientoId");

            migrationBuilder.RenameColumn(
                name: "Usuario_EstacionamientoESTACIONAMIENTO_ID",
                table: "AccionUsuarioEstacionamiento",
                newName: "Usuario_EstacionamientoUsuarioEstacionamientoId");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioEstacionamientoId",
                table: "UsuarioEstacionamientos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
                table: "UsuarioEstacionamientos",
                column: "UsuarioEstacionamientoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrupoUsuarioEstacionamiento",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "GruposGRU_ID", "Usuario_EstacionamientoUsuarioEstacionamientoId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccionUsuarioEstacionamiento",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "AccionesACC_ID", "Usuario_EstacionamientoUsuarioEstacionamientoId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEstacionamientos_ESTACIONAMIENTO_ID",
                table: "UsuarioEstacionamientos",
                column: "ESTACIONAMIENTO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEstacionamientos_USU_ID",
                table: "UsuarioEstacionamientos",
                column: "USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUsuarioE~",
                table: "GrupoUsuarioEstacionamiento",
                column: "Usuario_EstacionamientoUsuarioEstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUsuario~",
                table: "AccionUsuarioEstacionamiento",
                column: "Usuario_EstacionamientoUsuarioEstacionamientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccionUsuarioEstacionamiento_UsuarioEstacionamientos_Usuari~",
                table: "AccionUsuarioEstacionamiento",
                column: "Usuario_EstacionamientoUsuarioEstacionamientoId",
                principalTable: "UsuarioEstacionamientos",
                principalColumn: "UsuarioEstacionamientoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuarioEstacionamiento_UsuarioEstacionamientos_Usuario~",
                table: "GrupoUsuarioEstacionamiento",
                column: "Usuario_EstacionamientoUsuarioEstacionamientoId",
                principalTable: "UsuarioEstacionamientos",
                principalColumn: "UsuarioEstacionamientoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioEstacionamientos_Estacionamientos_ESTACIONAMIENTO_ID",
                table: "UsuarioEstacionamientos",
                column: "ESTACIONAMIENTO_ID",
                principalTable: "Estacionamientos",
                principalColumn: "EstacionamientoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccionUsuarioEstacionamiento_UsuarioEstacionamientos_Usuari~",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoUsuarioEstacionamiento_UsuarioEstacionamientos_Usuario~",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioEstacionamientos_Estacionamientos_ESTACIONAMIENTO_ID",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioEstacionamientos_ESTACIONAMIENTO_ID",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioEstacionamientos_USU_ID",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrupoUsuarioEstacionamiento",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUsuarioE~",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccionUsuarioEstacionamiento",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUsuario~",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.RenameColumn(
                name: "Usuario_EstacionamientoUsuarioEstacionamientoId",
                table: "GrupoUsuarioEstacionamiento",
                newName: "Usuario_EstacionamientoESTACIONAMIENTO_ID");

            migrationBuilder.RenameColumn(
                name: "Usuario_EstacionamientoUsuarioEstacionamientoId",
                table: "AccionUsuarioEstacionamiento",
                newName: "Usuario_EstacionamientoESTACIONAMIENTO_ID");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioEstacionamientoId",
                table: "UsuarioEstacionamientos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "EstacionamientoId",
                table: "UsuarioEstacionamientos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "GrupoUsuarioEstacionamiento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "AccionUsuarioEstacionamiento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
                table: "UsuarioEstacionamientos",
                columns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrupoUsuarioEstacionamiento",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "GruposGRU_ID", "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccionUsuarioEstacionamiento",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "AccionesACC_ID", "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEstacionamientos_EstacionamientoId",
                table: "UsuarioEstacionamientos",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_U~",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Cocheras_EstacionamientoId",
                table: "Cocheras",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_~",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccionUsuarioEstacionamiento_UsuarioEstacionamientos_Usuari~",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" },
                principalTable: "UsuarioEstacionamientos",
                principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoUsuarioEstacionamiento_UsuarioEstacionamientos_Usuario~",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" },
                principalTable: "UsuarioEstacionamientos",
                principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioEstacionamientos_Estacionamientos_EstacionamientoId",
                table: "UsuarioEstacionamientos",
                column: "EstacionamientoId",
                principalTable: "Estacionamientos",
                principalColumn: "EstacionamientoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
