using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModuloSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraActualizacion",
                table: "TarifasMensuales",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraCarga",
                table: "PagosMensuales",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "MovimientosCaja",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

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
                name: "Modulos",
                columns: table => new
                {
                    MOD_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MOD_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.MOD_ID);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    PER_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PER_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    USU_USUARIO = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    USU_CLAVE = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    USU_MAIL = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    EST_USU_ID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.PER_ID);
                    table.ForeignKey(
                        name: "FK_Personas_Estado_Usuario_EST_USU_ID",
                        column: x => x.EST_USU_ID,
                        principalTable: "Estado_Usuario",
                        principalColumn: "EST_USU_ID");
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    GRU_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GRU_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    GRU_DESCRIPCION = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EST_GRU_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.GRU_ID);
                    table.ForeignKey(
                        name: "FK_Grupos_Estados_Grupos_EST_GRU_ID",
                        column: x => x.EST_GRU_ID,
                        principalTable: "Estados_Grupos",
                        principalColumn: "EST_GRU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Formularios",
                columns: table => new
                {
                    FOR_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FOR_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    MOD_ID = table.Column<int>(type: "integer", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "GrupoUsuario",
                columns: table => new
                {
                    GruposGRU_ID = table.Column<int>(type: "integer", nullable: false),
                    UsuariosPER_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoUsuario", x => new { x.GruposGRU_ID, x.UsuariosPER_ID });
                    table.ForeignKey(
                        name: "FK_GrupoUsuario_Grupos_GruposGRU_ID",
                        column: x => x.GruposGRU_ID,
                        principalTable: "Grupos",
                        principalColumn: "GRU_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoUsuario_Personas_UsuariosPER_ID",
                        column: x => x.UsuariosPER_ID,
                        principalTable: "Personas",
                        principalColumn: "PER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Acciones",
                columns: table => new
                {
                    ACC_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ACC_NOMBRE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    FOR_ID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acciones", x => x.ACC_ID);
                    table.ForeignKey(
                        name: "FK_Acciones_Formularios_FOR_ID",
                        column: x => x.FOR_ID,
                        principalTable: "Formularios",
                        principalColumn: "FOR_ID");
                });

            migrationBuilder.CreateTable(
                name: "AccionGrupo",
                columns: table => new
                {
                    AccionesACC_ID = table.Column<int>(type: "integer", nullable: false),
                    GruposGRU_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionGrupo", x => new { x.AccionesACC_ID, x.GruposGRU_ID });
                    table.ForeignKey(
                        name: "FK_AccionGrupo_Acciones_AccionesACC_ID",
                        column: x => x.AccionesACC_ID,
                        principalTable: "Acciones",
                        principalColumn: "ACC_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccionGrupo_Grupos_GruposGRU_ID",
                        column: x => x.GruposGRU_ID,
                        principalTable: "Grupos",
                        principalColumn: "GRU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccionUsuario",
                columns: table => new
                {
                    AccionesACC_ID = table.Column<int>(type: "integer", nullable: false),
                    UsuariosPER_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionUsuario", x => new { x.AccionesACC_ID, x.UsuariosPER_ID });
                    table.ForeignKey(
                        name: "FK_AccionUsuario_Acciones_AccionesACC_ID",
                        column: x => x.AccionesACC_ID,
                        principalTable: "Acciones",
                        principalColumn: "ACC_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccionUsuario_Personas_UsuariosPER_ID",
                        column: x => x.UsuariosPER_ID,
                        principalTable: "Personas",
                        principalColumn: "PER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acciones_FOR_ID",
                table: "Acciones",
                column: "FOR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AccionGrupo_GruposGRU_ID",
                table: "AccionGrupo",
                column: "GruposGRU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuario_UsuariosPER_ID",
                table: "AccionUsuario",
                column: "UsuariosPER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Formularios_MOD_ID",
                table: "Formularios",
                column: "MOD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_EST_GRU_ID",
                table: "Grupos",
                column: "EST_GRU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuario_UsuariosPER_ID",
                table: "GrupoUsuario",
                column: "UsuariosPER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_EST_USU_ID",
                table: "Personas",
                column: "EST_USU_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccionGrupo");

            migrationBuilder.DropTable(
                name: "AccionUsuario");

            migrationBuilder.DropTable(
                name: "GrupoUsuario");

            migrationBuilder.DropTable(
                name: "Acciones");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "Formularios");

            migrationBuilder.DropTable(
                name: "Estados_Grupos");

            migrationBuilder.DropTable(
                name: "Estado_Usuario");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraActualizacion",
                table: "TarifasMensuales",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHoraCarga",
                table: "PagosMensuales",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "MovimientosCaja",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
