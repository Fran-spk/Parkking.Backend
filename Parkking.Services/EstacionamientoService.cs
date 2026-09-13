using Parkking.DTOs.Estacionamiento;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class EstacionamientoService
{
    private readonly EstacionamientoRepository _repository;
    private readonly MetodoDePagoService _metodoDePagoService;
    private readonly IEstacionamientoContext _estacionamientoContext;

    public EstacionamientoService(
        EstacionamientoRepository repository,
        MetodoDePagoService metodoDePagoService,
        IEstacionamientoContext estacionamientoContext)
    {
        _repository = repository;
        _metodoDePagoService = metodoDePagoService;
        _estacionamientoContext = estacionamientoContext;
    }

    private int TenantId => _estacionamientoContext.EstacionamientoId;

    public DatosEstacionamiento GetActual() =>
        _repository.GetById(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

    public void Crear(CrearEstacionamientoRequest request)
    {
        Validar(request);

        var datos = new DatosEstacionamiento
        {
            Nombre = request.Nombre.Trim(),
            Direccion = request.Direccion.Trim(),
            DiaVencimientoAbono = request.DiaVencimientoAbono,
            AplicaRecargo = request.AplicaRecargo,
            PorcentajeRecargo = request.AplicaRecargo ? request.PorcentajeRecargo : 0,
            DiasUmbralProporcional = request.DiasUmbralProporcional,
            ImprimirReciboAlCobrar = request.ImprimirReciboAlCobrar,
            Activo = true
        };

        _repository.Add(datos);
        _metodoDePagoService.SeedDefaults(datos.EstacionamientoId);
    }

    public void Editar(CrearEstacionamientoRequest request)
    {
        Validar(request);

        var datos = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        datos.Nombre = request.Nombre.Trim();
        datos.Direccion = request.Direccion.Trim();
        datos.DiaVencimientoAbono = request.DiaVencimientoAbono;
        datos.AplicaRecargo = request.AplicaRecargo;
        datos.PorcentajeRecargo = request.AplicaRecargo ? request.PorcentajeRecargo : 0;
        datos.DiasUmbralProporcional = request.DiasUmbralProporcional;
        datos.ImprimirReciboAlCobrar = request.ImprimirReciboAlCobrar;

        _repository.Update(datos);
    }

    public void BajaLogica()
    {
        var datos = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        datos.Activo = false;
        _repository.Update(datos);
    }

    private static void Validar(CrearEstacionamientoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new Exception("Debe ingresar un nombre.");

        if (string.IsNullOrWhiteSpace(request.Direccion))
            throw new Exception("Debe ingresar una dirección.");

        if (request.DiaVencimientoAbono < 1 || request.DiaVencimientoAbono > 31)
            throw new Exception("El día de vencimiento debe estar entre 1 y 31.");

        if (request.AplicaRecargo && request.PorcentajeRecargo <= 0)
            throw new Exception("El porcentaje de recargo debe ser mayor a cero.");

        if (!request.AplicaRecargo && request.PorcentajeRecargo != 0)
            throw new Exception("No puede ingresar un porcentaje si el recargo está deshabilitado.");

        if (request.DiasUmbralProporcional is < 1 or > 31)
            throw new Exception("El día umbral proporcional debe estar entre 1 y 31.");
    }
}
