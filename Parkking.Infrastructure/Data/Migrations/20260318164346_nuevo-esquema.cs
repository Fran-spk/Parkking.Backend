using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class nuevoesquema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estacionamientos",
                columns: table => new
                {
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    DiaVencimientoAbono = table.Column<int>(type: "integer", nullable: false),
                    AplicaRecargo = table.Column<bool>(type: "boolean", nullable: false),
                    PorcentajeRecargo = table.Column<decimal>(type: "numeric", nullable: false),
                    DiasUmbralProporcional = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estacionamientos", x => x.EstacionamientoId);
                });

            migrationBuilder.CreateTable(
                name: "CajasMensuales",
                columns: table => new
                {
                    CajaMensualId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<DateOnly>(type: "date", nullable: false),
                    Cerrada = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "CategoriasCochera",
                columns: table => new
                {
                    CategoriaCocheraId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasCochera", x => x.CategoriaCocheraId);
                    table.ForeignKey(
                        name: "FK_CategoriasCochera_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteId);
                    table.ForeignKey(
                        name: "FK_Clientes_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TiposVehiculo",
                columns: table => new
                {
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposVehiculo", x => x.TipoVehiculoId);
                    table.ForeignKey(
                        name: "FK_TiposVehiculo_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cocheras",
                columns: table => new
                {
                    CocheraId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    CategoriaCocheraId = table.Column<int>(type: "integer", nullable: false),
                    EstadoCochera = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    MultipleOcupacion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cocheras", x => x.CocheraId);
                    table.ForeignKey(
                        name: "FK_Cocheras_CategoriasCochera_CategoriaCocheraId",
                        column: x => x.CategoriaCocheraId,
                        principalTable: "CategoriasCochera",
                        principalColumn: "CategoriaCocheraId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cocheras_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TarifasMensuales",
                columns: table => new
                {
                    TarifaMensualId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionamientoId = table.Column<int>(type: "integer", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaHoraActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoriaCocheraId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasMensuales", x => x.TarifaMensualId);
                    table.ForeignKey(
                        name: "FK_TarifasMensuales_CategoriasCochera_CategoriaCocheraId",
                        column: x => x.CategoriaCocheraId,
                        principalTable: "CategoriasCochera",
                        principalColumn: "CategoriaCocheraId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TarifasMensuales_Estacionamientos_EstacionamientoId",
                        column: x => x.EstacionamientoId,
                        principalTable: "Estacionamientos",
                        principalColumn: "EstacionamientoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TarifasMensuales_TiposVehiculo_TipoVehiculoId",
                        column: x => x.TipoVehiculoId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "TipoVehiculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbonoCocheras",
                columns: table => new
                {
                    AbonoCocheraId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    CocheraId = table.Column<int>(type: "integer", nullable: false),
                    TipoVehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Patente = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ModeloVehiculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cobrador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    PrecioAcordado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CocheraTipoVehiculo",
                columns: table => new
                {
                    CocherasCocheraId = table.Column<int>(type: "integer", nullable: false),
                    VehiculosPermitidosTipoVehiculoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CocheraTipoVehiculo", x => new { x.CocherasCocheraId, x.VehiculosPermitidosTipoVehiculoId });
                    table.ForeignKey(
                        name: "FK_CocheraTipoVehiculo_Cocheras_CocherasCocheraId",
                        column: x => x.CocherasCocheraId,
                        principalTable: "Cocheras",
                        principalColumn: "CocheraId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CocheraTipoVehiculo_TiposVehiculo_VehiculosPermitidosTipoVe~",
                        column: x => x.VehiculosPermitidosTipoVehiculoId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "TipoVehiculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosMensuales",
                columns: table => new
                {
                    PagoMensualId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AbonoCocheraId = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaHoraCarga = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Recargo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MercadoPagoId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosMensuales", x => x.PagoMensualId);
                    table.ForeignKey(
                        name: "FK_PagosMensuales_AbonoCocheras_AbonoCocheraId",
                        column: x => x.AbonoCocheraId,
                        principalTable: "AbonoCocheras",
                        principalColumn: "AbonoCocheraId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    MovimientoCajaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CajaMensualId = table.Column<int>(type: "integer", nullable: false),
                    PagoMensualId = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Responsable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    TipoConcepto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.MovimientoCajaId);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_CajasMensuales_CajaMensualId",
                        column: x => x.CajaMensualId,
                        principalTable: "CajasMensuales",
                        principalColumn: "CajaMensualId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId");
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_PagosMensuales_PagoMensualId",
                        column: x => x.PagoMensualId,
                        principalTable: "PagosMensuales",
                        principalColumn: "PagoMensualId");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CajasMensuales_EstacionamientoId",
                table: "CajasMensuales",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasCochera_EstacionamientoId",
                table: "CategoriasCochera",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EstacionamientoId",
                table: "Clientes",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cocheras_CategoriaCocheraId",
                table: "Cocheras",
                column: "CategoriaCocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_Cocheras_EstacionamientoId",
                table: "Cocheras",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_CocheraTipoVehiculo_VehiculosPermitidosTipoVehiculoId",
                table: "CocheraTipoVehiculo",
                column: "VehiculosPermitidosTipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaMensualId",
                table: "MovimientosCaja",
                column: "CajaMensualId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_ClienteId",
                table: "MovimientosCaja",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_PagoMensualId",
                table: "MovimientosCaja",
                column: "PagoMensualId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosMensuales_AbonoCocheraId",
                table: "PagosMensuales",
                column: "AbonoCocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasMensuales_CategoriaCocheraId",
                table: "TarifasMensuales",
                column: "CategoriaCocheraId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasMensuales_EstacionamientoId",
                table: "TarifasMensuales",
                column: "EstacionamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasMensuales_TipoVehiculoId",
                table: "TarifasMensuales",
                column: "TipoVehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposVehiculo_EstacionamientoId",
                table: "TiposVehiculo",
                column: "EstacionamientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CocheraTipoVehiculo");

            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "TarifasMensuales");

            migrationBuilder.DropTable(
                name: "CajasMensuales");

            migrationBuilder.DropTable(
                name: "PagosMensuales");

            migrationBuilder.DropTable(
                name: "AbonoCocheras");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Cocheras");

            migrationBuilder.DropTable(
                name: "TiposVehiculo");

            migrationBuilder.DropTable(
                name: "CategoriasCochera");

            migrationBuilder.DropTable(
                name: "Estacionamientos");
        }
    }
}
