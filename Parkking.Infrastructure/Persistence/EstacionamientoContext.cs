using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Models.Finanzas;
using Parkking.Models.Seguridad;

namespace Parkking.Infrastructure.Persistence;

public class EstacionamientoContext : DbContext
{
    public int CurrentTenantId { get; }

    public EstacionamientoContext(
        DbContextOptions<EstacionamientoContext> options,
        IEstacionamientoContext estacionamientoContext)
        : base(options)
    {
        try
        {
            CurrentTenantId = estacionamientoContext.EstacionamientoId;
        }
        catch (UnauthorizedAccessException)
        {
            CurrentTenantId = 0;
        }
    }

    public DbSet<DatosEstacionamiento> Estacionamientos { get; set; } = null!;
    public DbSet<TipoVehiculo> TiposVehiculo { get; set; } = null!;
    public DbSet<TarifaMensual> TarifasMensuales { get; set; } = null!;
    public DbSet<Cochera> Cocheras { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Abono> Abonos { get; set; } = null!;
    public DbSet<AbonoPlaza> AbonoPlazas { get; set; } = null!;
    public DbSet<Vehiculo> Vehiculos { get; set; } = null!;
    public DbSet<AbonoVehiculo> AbonoVehiculos { get; set; } = null!;
    public DbSet<Cuota> Cuotas { get; set; } = null!;
    public DbSet<DetalleCuota> DetallesCuota { get; set; } = null!;
    public DbSet<Pago> Pagos { get; set; } = null!;
    public DbSet<DetallePago> DetallesPago { get; set; } = null!;
    public DbSet<Recibo> Recibos { get; set; } = null!;
    public DbSet<Mensaje> Mensajes { get; set; } = null!;
    public DbSet<Documento> Documentos { get; set; } = null!;
    public DbSet<MetodoDePago> MetodosDePago { get; set; } = null!;
    public DbSet<CuentaCorrienteCliente> CuentasCorrientesCliente { get; set; } = null!;
    public DbSet<CuentaCorrienteEstacionamiento> CuentasCorrientesEstacionamiento { get; set; } = null!;
    public DbSet<Movimiento> Movimientos { get; set; } = null!;
    public DbSet<GrupoFinanciero> GruposFinancieros { get; set; } = null!;
    public DbSet<MovimientoGrupoFinanciero> MovimientosGrupoFinanciero { get; set; } = null!;
    public DbSet<ReglaAsignacion> ReglasAsignacion { get; set; } = null!;
    public DbSet<TipoGasto> TiposGasto { get; set; } = null!;
    public DbSet<Cargo> Cargos { get; set; } = null!;
    public DbSet<Gasto> Gastos { get; set; } = null!;
    public DbSet<Reintegro> Reintegros { get; set; } = null!;
    public DbSet<AjusteFinanciero> AjustesFinancieros { get; set; } = null!;
    public DbSet<AplicacionSaldoAFavor> AplicacionesSaldoAFavor { get; set; } = null!;
    public DbSet<AuditoriaMovimiento> AuditoriasMovimiento { get; set; } = null!;
    public DbSet<CategoriaCochera> CategoriasCochera { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Grupo> Grupos { get; set; } = null!;
    public DbSet<Modulo> Modulos { get; set; } = null!;
    public DbSet<Accion> Acciones { get; set; } = null!;
    public DbSet<UsuarioEstacionamiento> UsuarioEstacionamientos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DatosEstacionamiento>(e =>
        {
            e.ToTable("Estacionamientos");
            e.HasKey(x => x.EstacionamientoId);
        });

        modelBuilder.Entity<Abono>(e =>
        {
            e.ToTable("Abonos");
            e.HasKey(x => x.AbonoId);
            e.HasOne(x => x.Cliente).WithMany(c => c.Abonos).HasForeignKey(x => x.ClienteId);
            e.Property(x => x.PeriodicidadCobro).HasConversion<int>();
            e.Property(x => x.PoliticaPrimerPeriodo).HasConversion<int>();
        });

        modelBuilder.Entity<AbonoPlaza>(e =>
        {
            e.ToTable("AbonoPlazas");
            e.HasKey(x => x.AbonoPlazaId);
            e.HasOne(x => x.Abono).WithMany(a => a.Plazas).HasForeignKey(x => x.AbonoId);
            e.HasOne(x => x.Cochera).WithMany(c => c.Plazas).HasForeignKey(x => x.CocheraId);
            e.HasIndex(x => new { x.AbonoId, x.CocheraId }).IsUnique();
        });

        modelBuilder.Entity<Vehiculo>(e =>
        {
            e.ToTable("Vehiculos");
            e.HasKey(x => x.VehiculoId);
            e.HasOne(x => x.Cliente).WithMany(c => c.Vehiculos).HasForeignKey(x => x.ClienteId);
            e.HasOne(x => x.TipoVehiculo).WithMany().HasForeignKey(x => x.TipoVehiculoId);
        });

        modelBuilder.Entity<AbonoVehiculo>(e =>
        {
            e.ToTable("AbonoVehiculos");
            e.HasKey(x => x.AbonoVehiculoId);
            e.HasOne(x => x.Abono).WithMany(a => a.AbonoVehiculos).HasForeignKey(x => x.AbonoId);
            e.HasOne(x => x.Vehiculo).WithOne(v => v.AbonoVehiculo)
                .HasForeignKey<AbonoVehiculo>(x => x.VehiculoId);
            e.HasOne(x => x.AbonoPlaza).WithMany(p => p.VehiculosFijos)
                .HasForeignKey(x => x.AbonoPlazaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.VehiculoId).IsUnique();
            e.Property(x => x.Modalidad).HasConversion<int>();
        });

        modelBuilder.Entity<Cuota>(e =>
        {
            e.ToTable("Cuotas");
            e.HasKey(x => x.CuotaId);
            e.HasOne(x => x.Abono).WithMany(a => a.Cuotas).HasForeignKey(x => x.AbonoId);
            e.Property(x => x.Estado).HasConversion<int>();
            e.HasIndex(x => new { x.AbonoId, x.PeriodoInicio }).IsUnique();
        });

        modelBuilder.Entity<DetalleCuota>(e =>
        {
            e.ToTable("DetallesCuota");
            e.HasKey(x => x.DetalleCuotaId);
            e.HasOne(x => x.Cuota).WithMany(c => c.Detalles).HasForeignKey(x => x.CuotaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tarifa).WithMany()
                .HasForeignKey(x => x.TarifaMensualId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Vehiculo).WithMany()
                .HasForeignKey(x => x.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Cochera).WithMany()
                .HasForeignKey(x => x.CocheraId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Tipo).HasConversion<int>();
            e.HasIndex(x => x.CuotaId);
        });

        modelBuilder.Entity<Pago>(e =>
        {
            e.ToTable("Pagos");
            e.HasKey(x => x.PagoId);
            e.HasOne(x => x.Abono).WithMany(a => a.Pagos).HasForeignKey(x => x.AbonoId);
            e.HasOne(x => x.MetodoDePago).WithMany()
                .HasForeignKey(x => x.MetodoDePagoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MetodoDePago>(e =>
        {
            e.ToTable("MetodosDePago");
            e.HasKey(x => x.MetodoDePagoId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.EstacionamientoId, x.Nombre });
        });

        modelBuilder.Entity<Recibo>(e =>
        {
            e.ToTable("Recibos");
            e.HasKey(x => x.ReciboId);
            e.HasOne(x => x.Pago).WithOne(p => p.Recibo)
                .HasForeignKey<Recibo>(x => x.PagoId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.PagoId).IsUnique();
            e.HasIndex(x => new { x.EstacionamientoId, x.Numero }).IsUnique();
        });

        modelBuilder.Entity<Mensaje>(e =>
        {
            e.ToTable("Mensajes");
            e.HasKey(x => x.MensajeId);
            e.HasIndex(x => new { x.EstacionamientoId, x.Fecha });
            e.HasIndex(x => x.ReciboId);
        });

        modelBuilder.Entity<Documento>(e =>
        {
            e.ToTable("Documentos");
            e.HasKey(x => x.DocumentoId);
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Vehiculo).WithMany().HasForeignKey(x => x.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Abono).WithMany().HasForeignKey(x => x.AbonoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EstacionamientoId, x.Activo, x.FechaCarga });
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.VehiculoId);
            e.HasIndex(x => x.AbonoId);
        });

        modelBuilder.Entity<DetallePago>(e =>
        {
            e.ToTable("DetallesPago");
            e.HasKey(x => x.DetallePagoId);
            e.HasOne(x => x.Pago).WithMany(p => p.Detalles).HasForeignKey(x => x.PagoId);
            e.HasOne(x => x.Cuota).WithMany(c => c.DetallesPago).HasForeignKey(x => x.CuotaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Movimiento>(e =>
        {
            e.ToTable("Movimientos");
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId);
            e.HasOne(x => x.Pago).WithMany().HasForeignKey(x => x.PagoId);
            e.HasOne(x => x.Abono).WithMany().HasForeignKey(x => x.AbonoId);
            e.HasOne(x => x.TipoGasto).WithMany().HasForeignKey(x => x.TipoGastoId);
            e.HasOne(x => x.CuentaCorrienteEstacionamiento).WithMany().HasForeignKey(x => x.CuentaCorrienteEstacionamientoId);
            e.HasOne(x => x.CuentaCorrienteCliente).WithMany().HasForeignKey(x => x.CuentaCorrienteClienteId);
            e.HasIndex(x => x.EstacionamientoId);
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.PagoId);
        });

        modelBuilder.Entity<CuentaCorrienteCliente>(e =>
        {
            e.ToTable("CuentasCorrientesCliente");
            e.HasIndex(x => x.ClienteId).IsUnique();
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId);
        });

        modelBuilder.Entity<CuentaCorrienteEstacionamiento>(e =>
        {
            e.ToTable("CuentasCorrientesEstacionamiento");
            e.HasIndex(x => x.EstacionamientoId).IsUnique();
        });

        modelBuilder.Entity<GrupoFinanciero>(e =>
        {
            e.ToTable("GruposFinancieros");
            e.HasIndex(x => new { x.EstacionamientoId, x.Nombre });
        });

        modelBuilder.Entity<MovimientoGrupoFinanciero>(e =>
        {
            e.ToTable("MovimientosGrupoFinanciero");
            e.HasKey(x => new { x.MovimientoId, x.GrupoFinancieroId });
            e.HasOne(x => x.Movimiento).WithMany(m => m.Grupos).HasForeignKey(x => x.MovimientoId);
            e.HasOne(x => x.GrupoFinanciero).WithMany(g => g.Movimientos).HasForeignKey(x => x.GrupoFinancieroId);
        });

        modelBuilder.Entity<ReglaAsignacion>(e =>
        {
            e.ToTable("ReglasAsignacion");
            e.HasOne(x => x.GrupoFinanciero).WithMany(g => g.Reglas).HasForeignKey(x => x.GrupoFinancieroId);
            e.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId);
            e.HasOne(x => x.TipoGasto).WithMany(t => t.Reglas).HasForeignKey(x => x.TipoGastoId);
        });

        modelBuilder.Entity<TipoGasto>(e => e.ToTable("TiposGasto"));
        modelBuilder.Entity<Cargo>(e =>
        {
            e.ToTable("Cargos");
            e.HasOne<Abono>().WithMany().HasForeignKey(x => x.AbonoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.AbonoId);
        });
        modelBuilder.Entity<Gasto>(e =>
        {
            e.ToTable("Gastos");
            e.HasOne(x => x.Movimiento).WithMany().HasForeignKey(x => x.MovimientoId);
            e.HasOne(x => x.TipoGasto).WithMany(t => t.Gastos).HasForeignKey(x => x.TipoGastoId);
        });
        modelBuilder.Entity<Reintegro>(e =>
        {
            e.ToTable("Reintegros");
            e.HasOne(x => x.Movimiento).WithMany().HasForeignKey(x => x.MovimientoId);
        });
        modelBuilder.Entity<AjusteFinanciero>(e =>
        {
            e.ToTable("AjustesFinancieros");
            e.HasOne(x => x.Movimiento).WithMany().HasForeignKey(x => x.MovimientoId);
        });
        modelBuilder.Entity<AplicacionSaldoAFavor>(e =>
        {
            e.ToTable("AplicacionesSaldoAFavor");
            e.HasOne(x => x.Movimiento).WithMany().HasForeignKey(x => x.MovimientoId);
        });
        modelBuilder.Entity<AuditoriaMovimiento>(e =>
        {
            e.ToTable("AuditoriasMovimiento");
            e.HasOne(x => x.Movimiento).WithMany(m => m.Auditorias).HasForeignKey(x => x.MovimientoId);
        });

        modelBuilder.Entity<UsuarioEstacionamiento>()
            .HasKey(ue => new { ue.USU_ID, ue.ESTACIONAMIENTO_ID });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantProperty = Expression.Property(parameter, nameof(IMultiTenant.EstacionamientoId));
            var contextParameter = Expression.Constant(this);
            var contextProperty = Expression.Property(contextParameter, nameof(CurrentTenantId));
            var body = Expression.Equal(tenantProperty, contextProperty);
            var lambda = Expression.Lambda(body, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToAddedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantIdToAddedEntities()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity is IMultiTenant))
        {
            var entity = (IMultiTenant)entry.Entity;
            if (entity.EstacionamientoId == 0)
                entity.EstacionamientoId = CurrentTenantId;
        }
    }
}
