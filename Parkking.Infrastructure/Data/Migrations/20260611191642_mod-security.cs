using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class modsecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cocheras_Estacionamientos_EstacionamientoId",
                table: "Cocheras");

            migrationBuilder.DropTable(
                name: "AccionUsuario");

            migrationBuilder.DropTable(
                name: "GrupoUsuario");

            migrationBuilder.AddColumn<int>(
                name: "AccionACC_ID",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrupoGRU_ID",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstacionamientoId",
                table: "PagosMensuales",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstacionamientoId",
                table: "MovimientosCaja",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoAnterior",
                table: "MovimientosCaja",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPosterior",
                table: "MovimientosCaja",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "MovimientosCaja",
                type: "integer",
                maxLength: 100,
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Estacionamientos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Saldo",
                table: "CajasMensuales",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGastos",
                table: "CajasMensuales",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIngresos",
                table: "CajasMensuales",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

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

            migrationBuilder.AddColumn<int>(
                name: "EstacionamientoId",
                table: "AbonoCocheras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UsuarioEstacionamientos",
                columns: table => new
                {
                    USU_ID = table.Column<int>(type: "integer", nullable: false),
                    ESTACIONAMIENTO_ID = table.Column<int>(type: "integer", nullable: false),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioEstacionamientos", x => new { x.USU_ID, x.ESTACIONAMIENTO_ID });
                    table.ForeignKey(
                        name: "FK_UsuarioEstacionamientos_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioEstacionamientos_Usuarios_USU_ID",
                        column: x => x.USU_ID,
                        principalTable: "Usuarios",
                        principalColumn: "USU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AccionACC_ID",
                table: "Usuarios",
                column: "AccionACC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GrupoGRU_ID",
                table: "Usuarios",
                column: "GrupoGRU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamientoE~",
                table: "Grupos",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Acciones_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamient~",
                table: "Acciones",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEstacionamientos_EstacionamientoId",
                table: "UsuarioEstacionamientos",
                column: "EstacionamientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Acciones_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_~",
                table: "Acciones",
                columns: new[] { "UsuarioEstacionamientoUSU_ID", "UsuarioEstacionamientoESTACIONAMIENTO_ID" },
                principalTable: "UsuarioEstacionamientos",
                principalColumns: new[] { "USU_ID", "ESTACIONAMIENTO_ID" });

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
                name: "FK_Usuarios_Grupos_GrupoGRU_ID",
                table: "Usuarios",
                column: "GrupoGRU_ID",
                principalTable: "Grupos",
                principalColumn: "GRU_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Acciones_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_~",
                table: "Acciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_UsuarioEstacionamientos_UsuarioEstacionamientoUSU_ID~",
                table: "Grupos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Acciones_AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Grupos_GrupoGRU_ID",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "UsuarioEstacionamientos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_GrupoGRU_ID",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamientoE~",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Acciones_UsuarioEstacionamientoUSU_ID_UsuarioEstacionamient~",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "AccionACC_ID",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "GrupoGRU_ID",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EstacionamientoId",
                table: "PagosMensuales");

            migrationBuilder.DropColumn(
                name: "EstacionamientoId",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "SaldoAnterior",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "SaldoPosterior",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Estacionamientos");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "CajasMensuales");

            migrationBuilder.DropColumn(
                name: "TotalGastos",
                table: "CajasMensuales");

            migrationBuilder.DropColumn(
                name: "TotalIngresos",
                table: "CajasMensuales");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoESTACIONAMIENTO_ID",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "UsuarioEstacionamientoUSU_ID",
                table: "Acciones");

            migrationBuilder.DropColumn(
                name: "EstacionamientoId",
                table: "AbonoCocheras");

            migrationBuilder.CreateTable(
                name: "AccionUsuario",
                columns: table => new
                {
                    AccionesACC_ID = table.Column<int>(type: "integer", nullable: false),
                    UsuariosUSU_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionUsuario", x => new { x.AccionesACC_ID, x.UsuariosUSU_ID });
                    table.ForeignKey(
                        name: "FK_AccionUsuario_Acciones_AccionesACC_ID",
                        column: x => x.AccionesACC_ID,
                        principalTable: "Acciones",
                        principalColumn: "ACC_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccionUsuario_Usuarios_UsuariosUSU_ID",
                        column: x => x.UsuariosUSU_ID,
                        principalTable: "Usuarios",
                        principalColumn: "USU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoUsuario",
                columns: table => new
                {
                    GruposGRU_ID = table.Column<int>(type: "integer", nullable: false),
                    UsuariosUSU_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoUsuario", x => new { x.GruposGRU_ID, x.UsuariosUSU_ID });
                    table.ForeignKey(
                        name: "FK_GrupoUsuario_Grupos_GruposGRU_ID",
                        column: x => x.GruposGRU_ID,
                        principalTable: "Grupos",
                        principalColumn: "GRU_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoUsuario_Usuarios_UsuariosUSU_ID",
                        column: x => x.UsuariosUSU_ID,
                        principalTable: "Usuarios",
                        principalColumn: "USU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuario_UsuariosUSU_ID",
                table: "AccionUsuario",
                column: "UsuariosUSU_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoUsuario_UsuariosUSU_ID",
                table: "GrupoUsuario",
                column: "UsuariosUSU_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cocheras_Estacionamientos_EstacionamientoId",
                table: "Cocheras",
                column: "EstacionamientoId",
                principalTable: "Estacionamientos",
                principalColumn: "EstacionamientoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
