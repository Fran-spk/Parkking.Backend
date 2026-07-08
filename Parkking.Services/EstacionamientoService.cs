using Parkking.DTOs.Estacionamiento;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class EstacionamientoService
{
    private readonly EstacionamientoRepository _repository;
    private readonly IEstacionamientoContext _estacionamientoContext;

    public EstacionamientoService(
        EstacionamientoRepository repository,
        IEstacionamientoContext estacionamientoContext)
    {
        _repository = repository;
        _estacionamientoContext = estacionamientoContext;
    }

    private int TenantId => _estacionamientoContext.EstacionamientoId;

    #region Consultas

    public Estacionamiento GetActual()
    {
        return _repository.GetById(TenantId)
            ?? throw new Exception("No se encontró el estacionamiento.");
    }

    #endregion

    #region Comandos

    public void Crear(CrearEstacionamientoRequest request)
    {
        Validar(request);

        var estacionamiento = new Estacionamiento
        {
            Nombre = request.Nombre.Trim(),
            Direccion = request.Direccion.Trim(),
            DiaVencimientoAbono = request.DiaVencimientoAbono,
            AplicaRecargo = request.AplicaRecargo,
            PorcentajeRecargo = request.PorcentajeRecargo,
            DiasUmbralProporcional = request.DiasUmbralProporcional,
            activo = true
        };

        _repository.Add(estacionamiento);
    }

    public void Editar(EditarEstacionamientoRequest request)
    {
        Validar(request);

        var estacionamiento = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontró el estacionamiento.");

        estacionamiento.Nombre = request.Nombre.Trim();
        estacionamiento.Direccion = request.Direccion.Trim();
        estacionamiento.DiaVencimientoAbono = request.DiaVencimientoAbono;
        estacionamiento.AplicaRecargo = request.AplicaRecargo;
        estacionamiento.PorcentajeRecargo = request.PorcentajeRecargo;
        estacionamiento.DiasUmbralProporcional = request.DiasUmbralProporcional;

        _repository.Update(estacionamiento);
    }

    public void BajaLogica()
    {
        var estacionamiento = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontró el estacionamiento.");

        estacionamiento.activo = false;
    }

    #endregion

    #region Validaciones

    private static void Validar(CrearEstacionamientoRequest request)
    {
        Validar(
            request.Nombre,
            request.Direccion,
            request.DiaVencimientoAbono,
            request.AplicaRecargo,
            request.PorcentajeRecargo,
            request.DiasUmbralProporcional);
    }

    private static void Validar(EditarEstacionamientoRequest request)
    {
        Validar(
            request.Nombre,
            request.Direccion,
            request.DiaVencimientoAbono,
            request.AplicaRecargo,
            request.PorcentajeRecargo,
            request.DiasUmbralProporcional);
    }

    private static void Validar(
        string nombre,
        string direccion,
        int diaVencimiento,
        bool aplicaRecargo,
        decimal porcentajeRecargo,
        int? diasUmbral)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("Debe ingresar un nombre.");

        if (string.IsNullOrWhiteSpace(direccion))
            throw new Exception("Debe ingresar una dirección.");

        if (diaVencimiento < 1 || diaVencimiento > 31)
            throw new Exception("El día de vencimiento debe estar entre 1 y 31.");

        if (aplicaRecargo && porcentajeRecargo <= 0)
            throw new Exception("El porcentaje de recargo debe ser mayor a cero.");

        if (!aplicaRecargo && porcentajeRecargo != 0)
            throw new Exception("No puede ingresar un porcentaje si el recargo está deshabilitado.");

        if (diasUmbral is < 1 or > 31)
            throw new Exception("El día umbral debe estar entre 1 y 31.");
    }

    #endregion
}