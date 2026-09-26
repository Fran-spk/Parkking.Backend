using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReemplazarCajaPorFinanzas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "CajasMensuales");

            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    CargoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.CargoId);
                    table.ForeignKey(
                        name: "FK_Cargos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuentasCorrientesCliente",
                columns: table => new
                {
                    CuentaCorrienteClienteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasCorrientesCliente", x => x.CuentaCorrienteClienteId);
                    table.ForeignKey(
                        name: "FK_CuentasCorrientesCliente_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuentasCorrientesEstacionamiento",
                columns: table => new
                {
                    CuentaCorrienteEstacionamientoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasCorrientesEstacionamiento", x => x.CuentaCorrienteEstacionamientoId);
                });

            migrationBuilder.CreateTable(
                name: "GruposFinancieros",
                columns: table => new
                {
                    GrupoFinancieroId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposFinancieros", x => x.GrupoFinancieroId);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    PagoId = table.Column<int>(type: "integer", nullable: true),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_Movimientos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId");
                    table.ForeignKey(
                        name: "FK_Movimientos_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "PagoId");
                });

            migrationBuilder.CreateTable(
                name: "TiposGasto",
                columns: table => new
                {
                    TipoGastoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposGasto", x => x.TipoGastoId);
                });

            migrationBuilder.CreateTable(
                name: "AjustesFinancieros",
                columns: table => new
                {
                    AjusteFinancieroId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesFinancieros", x => x.AjusteFinancieroId);
                    table.ForeignKey(
                        name: "FK_AjustesFinancieros_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AplicacionesSaldoAFavor",
                columns: table => new
                {
                    AplicacionSaldoAFavorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacionesSaldoAFavor", x => x.AplicacionSaldoAFavorId);
                    table.ForeignKey(
                        name: "FK_AplicacionesSaldoAFavor_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriasMovimiento",
                columns: table => new
                {
                    AuditoriaMovimientoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriasMovimiento", x => x.AuditoriaMovimientoId);
                    table.ForeignKey(
                        name: "FK_AuditoriasMovimiento_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosGrupoFinanciero",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    GrupoFinancieroId = table.Column<int>(type: "integer", nullable: false),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosGrupoFinanciero", x => new { x.MovimientoId, x.GrupoFinancieroId });
                    table.ForeignKey(
                        name: "FK_MovimientosGrupoFinanciero_GruposFinancieros_GrupoFinancier~",
                        column: x => x.GrupoFinancieroId,
                        principalTable: "GruposFinancieros",
                        principalColumn: "GrupoFinancieroId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosGrupoFinanciero_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reintegros",
                columns: table => new
                {
                    ReintegroId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    Beneficiario = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Medio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reintegros", x => x.ReintegroId);
                    table.ForeignKey(
                        name: "FK_Reintegros_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    GastoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    TipoGastoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    MovimientoId = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.GastoId);
                    table.ForeignKey(
                        name: "FK_Gastos_Movimientos_MovimientoId",
                        column: x => x.MovimientoId,
                        principalTable: "Movimientos",
                        principalColumn: "MovimientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gastos_TiposGasto_TipoGastoId",
                        column: x => x.TipoGastoId,
                        principalTable: "TiposGasto",
                        principalColumn: "TipoGastoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglasAsignacion",
                columns: table => new
                {
                    ReglaAsignacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    GrupoFinancieroId = table.Column<int>(type: "integer", nullable: false),
                    Criterio = table.Column<int>(type: "integer", nullable: false),
                    AbonoId = table.Column<int>(type: "integer", nullable: true),
                    TipoGastoId = table.Column<int>(type: "integer", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasAsignacion", x => x.ReglaAsignacionId);
                    table.ForeignKey(
                        name: "FK_ReglasAsignacion_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId");
                    table.ForeignKey(
                        name: "FK_ReglasAsignacion_GruposFinancieros_GrupoFinancieroId",
                        column: x => x.GrupoFinancieroId,
                        principalTable: "GruposFinancieros",
                        principalColumn: "GrupoFinancieroId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReglasAsignacion_TiposGasto_TipoGastoId",
                        column: x => x.TipoGastoId,
                        principalTable: "TiposGasto",
                        principalColumn: "TipoGastoId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesFinancieros_MovimientoId",
                table: "AjustesFinancieros",
                column: "MovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionesSaldoAFavor_MovimientoId",
                table: "AplicacionesSaldoAFavor",
                column: "MovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriasMovimiento_MovimientoId",
                table: "AuditoriasMovimiento",
                column: "MovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_ClienteId",
                table: "Cargos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasCorrientesCliente_ClienteId",
                table: "CuentasCorrientesCliente",
                column: "ClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasCorrientesEstacionamiento_EstacionamientoId",
                table: "CuentasCorrientesEstacionamiento",
                column: "EstacionamientoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_MovimientoId",
                table: "Gastos",
                column: "MovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_TipoGastoId",
                table: "Gastos",
                column: "TipoGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposFinancieros_EstacionamientoId_Nombre",
                table: "GruposFinancieros",
                columns: new[] { "EstacionamientoId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_ClienteId",
                table: "Movimientos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_EstacionamientoId",
                table: "Movimientos",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_PagoId",
                table: "Movimientos",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosGrupoFinanciero_GrupoFinancieroId",
                table: "MovimientosGrupoFinanciero",
                column: "GrupoFinancieroId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasAsignacion_AbonoId",
                table: "ReglasAsignacion",
                column: "AbonoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasAsignacion_GrupoFinancieroId",
                table: "ReglasAsignacion",
                column: "GrupoFinancieroId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasAsignacion_TipoGastoId",
                table: "ReglasAsignacion",
                column: "TipoGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reintegros_MovimientoId",
                table: "Reintegros",
                column: "MovimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjustesFinancieros");

            migrationBuilder.DropTable(
                name: "AplicacionesSaldoAFavor");

            migrationBuilder.DropTable(
                name: "AuditoriasMovimiento");

            migrationBuilder.DropTable(
                name: "Cargos");

            migrationBuilder.DropTable(
                name: "CuentasCorrientesCliente");

            migrationBuilder.DropTable(
                name: "CuentasCorrientesEstacionamiento");

            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "MovimientosGrupoFinanciero");

            migrationBuilder.DropTable(
                name: "ReglasAsignacion");

            migrationBuilder.DropTable(
                name: "Reintegros");

            migrationBuilder.DropTable(
                name: "GruposFinancieros");

            migrationBuilder.DropTable(
                name: "TiposGasto");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.CreateTable(
                name: "CajasMensuales",
                columns: table => new
                {
                    CajaMensualId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Cerrada = table.Column<bool>(type: "boolean", nullable: false),
                    Mes = table.Column<DateOnly>(type: "date", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalGastos = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalIngresos = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajasMensuales", x => x.CajaMensualId);
                    table.ForeignKey(
                        name: "FK_CajasMensuales_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    MovimientoCajaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AbonoId = table.Column<int>(type: "integer", nullable: true),
                    CajaMensualId = table.Column<int>(type: "integer", nullable: false),
                    PagoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Responsable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SaldoAnterior = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaldoPosterior = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    TipoConcepto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.MovimientoCajaId);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Abonos_AbonoId",
                        column: x => x.AbonoId,
                        principalTable: "Abonos",
                        principalColumn: "AbonoId");
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_CajasMensuales_CajaMensualId",
                        column: x => x.CajaMensualId,
                        principalTable: "CajasMensuales",
                        principalColumn: "CajaMensualId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "PagoId");
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "USU_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CajasMensuales_EstacionamientoId",
                table: "CajasMensuales",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_AbonoId",
                table: "MovimientosCaja",
                column: "AbonoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaMensualId",
                table: "MovimientosCaja",
                column: "CajaMensualId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_PagoId",
                table: "MovimientosCaja",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId");
        }
    }
}
