namespace Parkking_backend.Services
{
    using API.Controllers;
    using Microsoft.EntityFrameworkCore;
    using MODELO;
    using MODELO.Contexto;
    using System.Globalization;

    public class PagoService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public PagoService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        // =========================
        // REGISTRAR PAGO
        // =========================
        public void RegistrarPago(RegistrarPagoRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var abono = _context.AbonoCocheras
                    .Include(a => a.Cochera)
                    .FirstOrDefault(a => a.AbonoCocheraId == request.AbonoCocheraId && a.Activo);

                if (abono == null)
                    throw new Exception("Abono no encontrado");

                if (!DateOnly.TryParseExact(request.Mes, "yyyy-MM-dd", out DateOnly mesNormalizado))
                {
                    throw new Exception("El formato de fecha debe ser yyyy-MM-dd");
                }

                // CAMBIO AQUÍ: Ahora la validación de rango se hace contra FechaInicioCobro
                var mesInicioCobro = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);
                if (mesNormalizado < mesInicioCobro)
                    throw new Exception("No se puede registrar un pago anterior al inicio de los cobros del abono");

                var yaExiste = _context.PagosMensuales
                    .Any(p => p.AbonoCocheraId == request.AbonoCocheraId
                           && p.Mes.Year == mesNormalizado.Year
                           && p.Mes.Month == mesNormalizado.Month);

                if (yaExiste)
                    throw new Exception("Ya existe un pago registrado para ese mes");

                var pago = new PagoMensual
                {
                    AbonoCocheraId = request.AbonoCocheraId,
                    Mes = mesNormalizado,
                    FechaHoraCarga = DateTime.UtcNow,
                    Monto = request.Monto,
                    Recargo = request.Recargo,
                    Observacion = request.Observacion,
                    MercadoPagoId = request.MercadoPagoId,
                };

                _context.PagosMensuales.Add(pago);
                _context.SaveChanges();

                var caja = _context.CajasMensuales
                    .FirstOrDefault(c => c.Mes.Year == mesNormalizado.Year
                                      && c.Mes.Month == mesNormalizado.Month
                                      && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

                if (caja == null)
                {
                    caja = new Models.CajaMensual
                    {
                        EstacionamientoId = _estacionamiento.EstacionamientoId,
                        Mes = mesNormalizado,
                        Cerrada = false
                    };
                    _context.CajasMensuales.Add(caja);
                    _context.SaveChanges();
                }

                if (caja.Cerrada)
                    throw new Exception("La caja del mes está cerrada");

                Models.IMovimientoStrategy strategy = new Models.PagoMensualStrategy(
                    pago,
                    abono,
                    caja.CajaMensualId
                );

                var movimiento = strategy.CrearMovimiento();

                _context.MovimientosCaja.Add(movimiento);

                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // =========================
        // SUGERIDO
        // =========================
        public PagoSugeridoDto GetSugerido(int abonoCocheraId, DateOnly? mes)
        {
            var abono = _context.AbonoCocheras
                .Include(a => a.Cochera)
                .FirstOrDefault(a => a.AbonoCocheraId == abonoCocheraId && a.Activo);

            if (abono == null)
                throw new Exception("Abono no encontrado");

            var estacionamiento = _context.Estacionamientos
                .FirstOrDefault(e => e.EstacionamientoId == _estacionamiento.EstacionamientoId);

            var tarifaBase = abono.PrecioAcordado ?? ObtenerTarifaVigente(
                abono.TipoVehiculoId,
                abono.Cochera.CategoriaCocheraId,
                DateTime.UtcNow);

            var mesHoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var mesAPagar = mes.HasValue
                ? new DateOnly(mes.Value.Year, mes.Value.Month, 1)
                : new DateOnly(mesHoy.Year, mesHoy.Month, 1);

            // CAMBIO AQUÍ: Evaluamos las fechas contables contra FechaInicioCobro
            var mesInicioCobro = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);

            var aplicaProporcional = false;
            var monto = tarifaBase;

            // REGLA DE NEGOCIO: Solo aplica proporcional si el cobro inicia el mismo mes que la entrada física 
            // Y si la entrada física superó el umbral configurado.
            if (mesAPagar == mesInicioCobro &&
                abono.FechaInicio.Month == abono.FechaInicioCobro.Month &&
                abono.FechaInicio.Year == abono.FechaInicioCobro.Year &&
                estacionamiento?.DiasUmbralProporcional != null)
            {
                var dia = abono.FechaInicio.Day;
                if (dia > estacionamiento.DiasUmbralProporcional)
                {
                    var diasMes = DateTime.DaysInMonth(abono.FechaInicio.Year, abono.FechaInicio.Month);
                    var diasRestantes = diasMes - dia + 1;
                    monto = Math.Round((tarifaBase / diasMes) * diasRestantes, 0);
                    aplicaProporcional = true;
                }
            }

            // Si el operador marcó "Cobrar mes siguiente", para ese mes contable sugerirá el 100% de la tarifa base (no aplica proporcional)

            decimal recargo = 0;
            if (estacionamiento?.AplicaRecargo == true)
            {
                var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
                var vencimiento = new DateOnly(mesAPagar.Year, mesAPagar.Month, estacionamiento.DiaVencimientoAbono);
                if (hoy > vencimiento)
                    recargo = Math.Round(monto * estacionamiento.PorcentajeRecargo / 100, 2);
            }

            return new PagoSugeridoDto
            {
                Monto = monto,
                Recargo = recargo,
                AplicaProporcional = aplicaProporcional,
                EsPrecioAcordado = abono.PrecioAcordado.HasValue,
                MesDate = mesAPagar
            };
        }

        // =========================
        // GET DEUDA
        // =========================
        public DeudaClienteDto GetDeuda(int clienteId)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.ClienteId == clienteId)
                ?? throw new Exception("Cliente no encontrado");

            var abonos = _context.AbonoCocheras
                .Include(a => a.Cochera)
                .Include(a => a.TipoVehiculo)
                .Include(a => a.PagosMensuales)
                .Where(a => a.ClienteId == clienteId && a.Activo)
                .ToList();

            var mesesImpagos = new List<MesImpagoDto>();
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var abono in abonos)
            {
                // CAMBIO AQUÍ: El cursor del generador de deuda arranca en FechaInicioCobro.
                // Si se pateó el cobro al mes siguiente, el mes actual nunca va a entrar a este bucle.
                var cursor = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);
                var limite = new DateOnly(hoy.Year, hoy.Month, 1);

                while (cursor <= limite)
                {
                    var tienePago = abono.PagosMensuales
                        .Any(p => p.Mes.Year == cursor.Year && p.Mes.Month == cursor.Month);

                    if (!tienePago)
                    {
                        var monto = abono.PrecioAcordado ?? ObtenerTarifaVigente(
                            abono.TipoVehiculoId,
                            abono.Cochera.CategoriaCocheraId,
                            DateTime.UtcNow); // Ajuste prolijo por si no tiene precio acordado guardado fijo

                        mesesImpagos.Add(new MesImpagoDto
                        {
                            AbonoCocheraId = abono.AbonoCocheraId,
                            NumeroCochera = abono.Cochera.Numero,
                            TipoVehiculo = abono.TipoVehiculo.Nombre,
                            Mes = cursor.ToString("MMMM yyyy", new CultureInfo("es-AR")),
                            MesDate = cursor,
                            Monto = monto,
                            Recargo = 0,
                            Total = monto
                        });
                    }

                    cursor = cursor.AddMonths(1);
                }
            }

            return new DeudaClienteDto
            {
                ClienteId = clienteId,
                NombreCliente = cliente.Nombre,
                MesesImpagos = mesesImpagos
            };
        }

        public List<PagoMensual> GetByAbono(int abonoId)
        {
            return _context.PagosMensuales
                .Where(p => p.AbonoCocheraId == abonoId)
                .OrderByDescending(p => p.Mes)
                .ToList();
        }

        public List<PagoMensual> GetByCliente(int clienteId)
        {
            return _context.PagosMensuales
                .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
                .Include(p => p.AbonoCochera).ThenInclude(a => a.TipoVehiculo)
                .Where(p => p.AbonoCochera.ClienteId == clienteId)
                .OrderByDescending(p => p.Mes)
                .ToList();
        }

        public List<PagoMensual> GetAll(DateOnly? desde, DateOnly? hasta)
        {
            var query = _context.PagosMensuales
                .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
                .Include(p => p.AbonoCochera).ThenInclude(a => a.Cliente)
                .AsQueryable();

            if (desde.HasValue)
                query = query.Where(p => p.Mes >= desde);

            if (hasta.HasValue)
                query = query.Where(p => p.Mes <= hasta);

            return query.OrderByDescending(p => p.FechaHoraCarga).ToList();
        }

        private decimal ObtenerTarifaVigente(int tipoVehiculoId, int categoriaId, DateTime referencia)
        {
            return _context.TarifasMensuales
                .Where(t => t.TipoVehiculoId == tipoVehiculoId
                         && t.CategoriaCocheraId == categoriaId
                         && t.EstacionamientoId == _estacionamiento.EstacionamientoId
                         && t.FechaHoraActualizacion <= referencia)
                .OrderByDescending(t => t.FechaHoraActualizacion)
                .Select(t => t.Precio)
                .FirstOrDefault();
        }
    }
}