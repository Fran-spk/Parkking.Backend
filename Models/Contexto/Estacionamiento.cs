using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.seguridad;
using Modelo_Ids;
using Parkking_backend.Models;

namespace MODELO.Contexto
{
    // Servicio que provee el EstacionamientoId activo
    // Por ahora se setea desde header, cuando haya JWT se lee del claim
    public interface IEstacionamientoContext
    {
        int EstacionamientoId { get; }
    }

    public class EstacionamientoContextService : IEstacionamientoContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EstacionamientoContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int EstacionamientoId
        {
            get
            {
                // Cuando haya JWT reemplazar por:
                // var claim = _httpContextAccessor.HttpContext?.User
                //     .FindFirst("EstacionamientoId")?.Value;
                // return int.Parse(claim ?? "0");

                // Por ahora lee del header para desarrollo
                var header = _httpContextAccessor.HttpContext?.Request
                    .Headers["X-Estacionamiento-Id"].FirstOrDefault();

                return int.TryParse(header, out var id) ? id : 1;
            }
        }
    }

    public class EstacionamientoContext : DbContext
    {
        private readonly IEstacionamientoContext _estacionamientoContext;

        public EstacionamientoContext(
            DbContextOptions<EstacionamientoContext> options,
            IEstacionamientoContext estacionamientoContext)
            : base(options)
        {
            _estacionamientoContext = estacionamientoContext;
        }

        // ─────────────────────────────────────────
        // MÓDULO ABONOS
        // ─────────────────────────────────────────
        public DbSet<Estacionamiento> Estacionamientos { get; set; }
        public DbSet<TipoVehiculo> TiposVehiculo { get; set; }
        public DbSet<TarifaMensual> TarifasMensuales { get; set; }
        public DbSet<Cochera> Cocheras { get; set; }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<AbonoCochera> AbonoCocheras { get; set; }
        public DbSet<PagoMensual> PagosMensuales { get; set; }
        public DbSet<CajaMensual> CajasMensuales { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<CategoriaCochera> CategoriasCochera { get; set; }

        public DbSet<Estado_Grupo> Estados_Grupos { get; set; }

        // ─────────────────────────────────────────
        // MÓDULO SEGURIDAD
        // ─────────────────────────────────────────

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Formulario> Formularios { get; set; }
        public DbSet<Accion> Acciones { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var estId = _estacionamientoContext.EstacionamientoId;

            // ─────────────────────────────────────────
            // GLOBAL QUERY FILTERS — entidades raíz
            // ─────────────────────────────────────────
            modelBuilder.Entity<TipoVehiculo>()
                .HasQueryFilter(t => t.EstacionamientoId == estId);

            modelBuilder.Entity<TarifaMensual>()
                .HasQueryFilter(t => t.EstacionamientoId == estId);

            modelBuilder.Entity<Cochera>()
                .HasQueryFilter(c => c.EstacionamientoId == estId);

            modelBuilder.Entity<Cliente>()
                .HasQueryFilter(c => c.EstacionamientoId == estId);

            modelBuilder.Entity<CajaMensual>()
                .HasQueryFilter(c => c.EstacionamientoId == estId);

            // ─────────────────────────────────────────
            // ÍNDICES — performance por estacionamiento
            // ─────────────────────────────────────────
            modelBuilder.Entity<TipoVehiculo>()
                .HasIndex(t => t.EstacionamientoId);

            modelBuilder.Entity<TarifaMensual>()
                .HasIndex(t => t.EstacionamientoId);

            modelBuilder.Entity<Cochera>()
                .HasIndex(c => c.EstacionamientoId);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.EstacionamientoId);

            modelBuilder.Entity<CajaMensual>()
                .HasIndex(c => c.EstacionamientoId);
            modelBuilder.Entity<AbonoCochera>()
             .HasQueryFilter(a => a.Cliente.EstacionamientoId == estId);

            modelBuilder.Entity<PagoMensual>()
                .HasQueryFilter(p => p.AbonoCochera.Cliente.EstacionamientoId == estId);

            modelBuilder.Entity<MovimientoCaja>()
                .HasQueryFilter(m => m.CajaMensual.EstacionamientoId == estId);
        }
    }
}