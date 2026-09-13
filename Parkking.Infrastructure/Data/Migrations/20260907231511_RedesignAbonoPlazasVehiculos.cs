using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RedesignAbonoPlazasVehiculos : Migration
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
                name: "FK_MovimientosCaja_AbonoCocheras_AbonoCocheraId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosMensuales_AbonoCocheras_AbonoCocheraId",
                table: "PagosMensuales");

            // ─── Crear Abonos y copiar datos preservando IDs ───
            migrationBuilder.CreateTable(
                name: "Abonos",
                columns: table => new
                {
                    AbonoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Cobrador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaInicioCobro = table.Column<DateOnly>(type: "date", nullable: false),
                    PrecioAcordado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abonos", x => x.AbonoId);
                    table.ForeignKey(
                        name: "FK_Abonos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Abonos_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Abonos" ("AbonoId", "EstacionamientoId", "ClienteId", "Cobrador", "FechaInicio", "FechaInicioCobro", "PrecioAcordado", "Activo")
                OVERRIDING SYSTEM VALUE
                SELECT "AbonoCocheraId", "EstacionamientoId", "ClienteId", "Cobrador", "FechaInicio", "FechaInicioCobro", "PrecioAcordado", "Activo"
                FROM "AbonoCocheras";
                SELECT setval(pg_get_serial_sequence('"Abonos"', 'AbonoId'), COALESCE((SELECT MAX("AbonoId") FROM "Abonos"), 1));
                """);

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    VehiculoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Patente = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ModeloVehiculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.VehiculoId);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_TiposVehiculo_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "TipoVehiculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbonoPlazas",
                columns: table => new
                {
                    AbonoPlazaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    CocheraId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonoPlazas", x => x.AbonoPlazaId);
                    table.ForeignKey(
                        name: "FK_AbonoPlazas_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbonoPlazas_Cocheras_CocheraId",
                        column: x => x.CocheraId,
                        principalTable: "Cocheras",
                        principalColumn: "CocheraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "AbonoPlazas" ("EstacionamientoId", "AbonoId", "CocheraId", "Activo")
                SELECT "EstacionamientoId", "AbonoCocheraId", "CocheraId", TRUE
                FROM "AbonoCocheras";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Vehiculos" ("EstacionamientoId", "ClienteId", "Patente", "ModeloVehiculo", "TipoVehiculoId", "Activo")
                SELECT DISTINCT ON ("EstacionamientoId", "ClienteId", "Patente")
                       "EstacionamientoId", "ClienteId", "Patente", "ModeloVehiculo", "TipoVehiculoId", TRUE
                FROM "AbonoCocheras"
                WHERE "Patente" IS NOT NULL AND TRIM("Patente") <> ''
                ORDER BY "EstacionamientoId", "ClienteId", "Patente", "AbonoCocheraId";
                """);

            migrationBuilder.CreateTable(
                name: "AbonoVehiculos",
                columns: table => new
                {
                    AbonoVehiculoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: false),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Modalidad = table.Column<int>(type: "integer", nullable: false),
                    AbonoPlazaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonoVehiculos", x => x.AbonoVehiculoId);
                    table.ForeignKey(
                        name: "FK_AbonoVehiculos_AbonoPlazas_AbonoPlazaId",
                        column: x => x.AbonoPlazaId,
                        principalTable: "AbonoPlazas",
                        principalColumn: "AbonoPlazaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonoVehiculos_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbonoVehiculos_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "VehiculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "AbonoVehiculos" ("EstacionamientoId", "AbonoId", "VehiculoId", "Modalidad", "AbonoPlazaId")
                SELECT a."EstacionamientoId", a."AbonoCocheraId", v."VehiculoId", 0, p."AbonoPlazaId"
                FROM "AbonoCocheras" a
                INNER JOIN "Vehiculos" v
                    ON v."ClienteId" = a."ClienteId"
                   AND v."Patente" = a."Patente"
                   AND v."EstacionamientoId" = a."EstacionamientoId"
                INNER JOIN "AbonoPlazas" p
                    ON p."AbonoId" = a."AbonoCocheraId"
                   AND p."CocheraId" = a."CocheraId"
                WHERE a."Patente" IS NOT NULL AND TRIM(a."Patente") <> '';
                """);

            migrationBuilder.DropTable(
                name: "AbonoCocheras");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
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
                name: "AbonoCocheraId",
                table: "PagosMensuales",
                newName: "AbonoId");

            migrationBuilder.RenameIndex(
                name: "IX_PagosMensuales_AbonoCocheraId",
                table: "PagosMensuales",
                newName: "IX_PagosMensuales_AbonoId");

            migrationBuilder.RenameColumn(
                name: "AbonoCocheraId",
                table: "MovimientosCaja",
                newName: "AbonoId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_AbonoCocheraId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_AbonoId");

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
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_U~",
                table: "GrupoUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_~",
                table: "AccionUsuarioEstacionamiento",
                columns: new[] { "Usuario_EstacionamientoUSU_ID", "Usuario_EstacionamientoESTACIONAMIENTO_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_AbonoPlazas_AbonoId_CocheraId",
                table: "AbonoPlazas",
                columns: new[] { "AbonoId", "CocheraId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbonoPlazas_CocheraId",
                table: "AbonoPlazas",
                column: "CocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_Abonos_ClienteId",
                table: "Abonos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Abonos_EstacionamientoId",
                table: "Abonos",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonoVehiculos_AbonoId",
                table: "AbonoVehiculos",
                column: "AbonoId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonoVehiculos_AbonoPlazaId",
                table: "AbonoVehiculos",
                column: "AbonoPlazaId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonoVehiculos_VehiculoId",
                table: "AbonoVehiculos",
                column: "VehiculoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_ClienteId",
                table: "Vehiculos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_EstacionamientoId",
                table: "Vehiculos",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_TipoVehiculoId",
                table: "Vehiculos",
                column: "TipoVehiculoId");

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
                name: "FK_MovimientosCaja_Abonos_AbonoId",
                table: "MovimientosCaja",
                column: "AbonoId",
                principalTable: "Abonos",
                principalColumn: "AbonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosMensuales_Abonos_AbonoId",
                table: "PagosMensuales",
                column: "AbonoId",
                principalTable: "Abonos",
                principalColumn: "AbonoId",
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
                name: "FK_MovimientosCaja_Abonos_AbonoId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosMensuales_Abonos_AbonoId",
                table: "PagosMensuales");

            migrationBuilder.DropTable(
                name: "AbonoVehiculos");

            migrationBuilder.DropTable(
                name: "AbonoPlazas");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Abonos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioEstacionamientos",
                table: "UsuarioEstacionamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrupoUsuarioEstacionamiento",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_GrupoUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_U~",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccionUsuarioEstacionamiento",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropIndex(
                name: "IX_AccionUsuarioEstacionamiento_Usuario_EstacionamientoUSU_ID_~",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.DropColumn(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "GrupoUsuarioEstacionamiento");

            migrationBuilder.DropColumn(
                name: "Usuario_EstacionamientoUSU_ID",
                table: "AccionUsuarioEstacionamiento");

            migrationBuilder.RenameColumn(
                name: "AbonoId",
                table: "PagosMensuales",
                newName: "AbonoCocheraId");

            migrationBuilder.RenameIndex(
                name: "IX_PagosMensuales_AbonoId",
                table: "PagosMensuales",
                newName: "IX_PagosMensuales_AbonoCocheraId");

            migrationBuilder.RenameColumn(
                name: "AbonoId",
                table: "MovimientosCaja",
                newName: "AbonoCocheraId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_AbonoId",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_AbonoCocheraId");

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

            migrationBuilder.CreateTable(
                name: "AbonoCocheras",
                columns: table => new
                {
                    AbonoCocheraId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    CocheraId = table.Column<int>(type: "integer", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Cobrador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaInicioCobro = table.Column<DateOnly>(type: "date", nullable: false),
                    ModeloVehiculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Patente = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PrecioAcordado = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonoCocheras", x => x.AbonoCocheraId);
                    table.ForeignKey(
                        name: "FK_AbonoCocheras_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbonoCocheras_Cocheras_CocheraId",
                        column: x => x.CocheraId,
                        principalTable: "Cocheras",
                        principalColumn: "CocheraId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbonoCocheras_TiposVehiculo_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "TipoVehiculoId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AbonoCocheras_ClienteId",
                table: "AbonoCocheras",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonoCocheras_CocheraId",
                table: "AbonoCocheras",
                column: "CocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonoCocheras_TipoVehiculoId",
                table: "AbonoCocheras",
                column: "TipoVehiculoId");

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
                name: "FK_MovimientosCaja_AbonoCocheras_AbonoCocheraId",
                table: "MovimientosCaja",
                column: "AbonoCocheraId",
                principalTable: "AbonoCocheras",
                principalColumn: "AbonoCocheraId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosMensuales_AbonoCocheras_AbonoCocheraId",
                table: "PagosMensuales",
                column: "AbonoCocheraId",
                principalTable: "AbonoCocheras",
                principalColumn: "AbonoCocheraId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
