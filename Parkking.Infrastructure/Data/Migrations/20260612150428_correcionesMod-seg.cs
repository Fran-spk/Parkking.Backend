using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class correcionesModseg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Acciones_Formularios_FOR_ID",
                table: "Acciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Acciones_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_~",
                table: "Acciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_Estados_Grupos_EST_GRU_ID",
                table: "Grupos");

            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_ID~",
                table: "Grupos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Acciones_AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Estado_Usuario_EST_USU_ID",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Grupos_GrupoGRU_ID",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Estado_Usuario");

            migrationBuilder.DropTable(
                name: "Estados_Grupos");

            migrationBuilder.DropTable(
                name: "Formularios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EST_USU_ID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_GrupoGRU_ID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_EST_GRU_ID",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamientoE~",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Acciones_FOR_ID",
                table: "Acciones");

            migrationBuilder.DropIndex(
                name: "IX_Acciones_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamient~",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EST_USU_ID",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "FOR_ID",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Acciones");

            migrationBuilder.RenameColumn(
                name: "GrupoGRU_ID",
                table: "Usuarios",
                newName: "Estado_Usuario");

            migrationBuilder.RenameColumn(
                name: "EST_GRU_ID",
                table: "Grupos",
                newName: "EstadoGrupo");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEstacionamientoId",
                table: "UsuarioEstacionamientos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccionUsuarioEstacionamiento",
                columns: table => new
                {
                    AccionesACC_ID = table.Column<int>(type: "integer", nullable: false),
                    Usuario_EstacionamientoUSU_ID = table.Column<int>(type: "integer", nullable: false),
                    Usuario_EstacionamientoESTACIONAMIENTO_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionUsuarioEstacionamiento", x => new { x.AccionesACC_ID, x.Usuario_EstacionamientoUSU_ID, x.Usuario_EstacionamientoESTACIONAMIENTO_ID });
                    table.ForeignKey(
                        name: "FK_AccionUsuarioEstacionamiento_Acciones_AccionesACC_ID",
                        column: x => x.AccionesACC_ID,
                        principalTable: "Acciones",
                        principalColumn: "ACC_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccionUsuarioEstacionamiento_UsuarioEstacionamientos_Usuari~",
                        columns: x => new { x.Usuario_EstacionamientoUSU_ID, x.Usuario_EstacionamientoESTACIONAMIENTO_ID },
                        principalTable: "UsuarioEstacionamientos",
                        principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoUsuarioEstacionamiento",
                columns: table => new
                {
                    GruposGRU_ID = table.Column<int>(type: "integer", nullable: false),
                    Usuario_EstacionamientoUSU_ID = table.Column<int>(type: "integer", nullable: false),
                    Usuario_EstacionamientoESTACIONAMIENTO_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoUsuarioEstacionamiento", x => new { x.GruposGRU_ID, x.Usuario_EstacionamientoUSU_ID, x.Usuario_EstacionamientoESTACIONAMIENTO_ID });
                    table.ForeignKey(
                        name: "FK_GrupoUsuarioEstacionamiento_Grupos_GruposGRU_ID",
                        column: x => x.GruposGRU_ID,
                        principalTable: "Grupos",
                        principalColumn: "GRU_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoUsuarioEstacionamiento_UsuarioEstacionamientos_Usuario~",
                        columns: x => new { x.Usuario_EstacionamientoUSU_ID, x.Usuario_EstacionamientoESTACIONAMIENTO_ID },
                        principalTable: "UsuarioEstacionamientos",
                        principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_~",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_U~",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropTable(
                name: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoId",
                table: "UsuarioEstacionamientos");

            migrationBuilder.RenameColumn(
                name: "Estado_Usuario",
                table: "Usuarios",
                newName: "GrupoGRU_ID");

            migrationBuilder.RenameColumn(
                name: "EstadoGrupo",
                table: "Grupos",
                newName: "EST_GRU_ID");

            migrationBuilder.AddColumn<int>(
                name: "AccionACC_ID",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EST_USU_ID",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Grupos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Grupos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FOR_ID",
                table: "Acciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Acciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Acciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Estado_Usuario",
                columns: table => new
                {
                    EST_USU_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EST_USU_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estado_Usuario", x => x.EST_USU_ID);
                });

            migrationBuilder.CreateTable(
                name: "Estados_Grupos",
                columns: table => new
                {
                    EST_GRU_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EST_GRU_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados_Grupos", x => x.EST_GRU_ID);
                });

            migrationBuilder.CreateTable(
                name: "Formularios",
                columns: table => new
                {
                    FOR_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MOD_ID = table.Column<int>(type: "integer", nullable: true),
                    FOR_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formularios", x => x.FOR_ID);
                    table.ForeignKey(
                        name: "FK_Formularios_Modulos_MOD_ID",
                        column: x => x.MOD_ID,
                        principalTable: "Modulos",
                        principalColumn: "MOD_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AccionACC_ID",
                table: "Usuarios",
                column: "AccionACC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EST_USU_ID",
                table: "Usuarios",
                column: "EST_USU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GrupoGRU_ID",
                table: "Usuarios",
                column: "GrupoGRU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_EST_GRU_ID",
                table: "Grupos",
                column: "EST_GRU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamientoE~",
                table: "Grupos",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Acciones_FOR_ID",
                table: "Acciones",
                column: "FOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Acciones_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamient~",
                table: "Acciones",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Formularios_MOD_ID",
                table: "Formularios",
                column: "MOD_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Acciones_Formularios_FOR_ID",
                table: "Acciones",
                column: "FOR_ID",
                principalTable: "Formularios",
                principalColumn: "FOR_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Acciones_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_~",
                table: "Acciones",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" },
                principalTable: "UsuarioEstacionamientos",
                principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_Estados_Grupos_EST_GRU_ID",
                table: "Grupos",
                column: "EST_GRU_ID",
                principalTable: "Estados_Grupos",
                principalColumn: "EST_GRU_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_ID~",
                table: "Grupos",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" },
                principalTable: "UsuarioEstacionamientos",
                principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Acciones_AccionACC_ID",
                table: "Usuarios",
                column: "AccionACC_ID",
                principalTable: "Acciones",
                principalColumn: "ACC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Estado_Usuario_EST_USU_ID",
                table: "Usuarios",
                column: "EST_USU_ID",
                principalTable: "Estado_Usuario",
                principalColumn: "EST_USU_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Grupos_GrupoGRU_ID",
                table: "Usuarios",
                column: "GrupoGRU_ID",
                principalTable: "Grupos",
                principalColumn: "GRU_ID");
        }
    }
}
