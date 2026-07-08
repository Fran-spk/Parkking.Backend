using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
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

    public DbSet<Estacionamiento> Estacionamientos { get; set; } = null!;
    public DbSet<TipoVehiculo> TiposVehiculo { get; set; } = null!;
    public DbSet<TarifaMensual> TarifasMensuales { get; set; } = null!;
    public DbSet<Cochera> Cocheras { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<AbonoCochera> AbonoCocheras { get; set; } = null!;
    public DbSet<PagoMensual> PagosMensuales { get; set; } = null!;
    public DbSet<CajaMensual> CajasMensuales { get; set; } = null!;
    public DbSet<MovimientoCaja> MovimientosCaja { get; set; } = null!;
    public DbSet<CategoriaCochera> CategoriasCochera { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Grupo> Grupos { get; set; } = null!;
    public DbSet<Modulo> Modulos { get; set; } = null!;
    public DbSet<Accion> Acciones { get; set; } = null!;
    public DbSet<UsuarioEstacionamiento> UsuarioEstacionamientos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
                continue;

            // REFACTORIZACIÓN DE LA EXPRESIÓN:
            // En lugar de pasar un valor constante fijo, hacemos que apunte a la propiedad 
            // 'CurrentTenantId' de la instancia actual del DbContext en cada ejecución.
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantProperty = Expression.Property(parameter, nameof(IMultiTenant.EstacionamientoId));

            // Accedemos a 'this.CurrentTenantId' de manera diferida
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
