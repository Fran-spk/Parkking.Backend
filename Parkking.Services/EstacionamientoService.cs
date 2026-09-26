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

    public DatosEstacionamiento Crear(CrearEstacionamientoRequest request)
    {
        Validar(
            request.Nombre,
            request.Direccion,
            request.EmailAvisos,
            request.DiaVencimientoAbono,
            request.AplicaRecargo,
            request.PorcentajeRecargo,
            request.ContratoPlazoMeses);

        var datos = new DatosEstacionamiento
        {
            Nombre = request.Nombre.Trim(),
            Direccion = request.Direccion.Trim(),
            LocadorNombre = string.IsNullOrWhiteSpace(request.LocadorNombre) ? null : request.LocadorNombre.Trim(),
            LocadorDocumento = string.IsNullOrWhiteSpace(request.LocadorDocumento) ? null : request.LocadorDocumento.Trim(),
            LocadorDomicilio = string.IsNullOrWhiteSpace(request.LocadorDomicilio) ? null : request.LocadorDomicilio.Trim(),
            DiaVencimientoAbono = request.DiaVencimientoAbono,
            AplicaRecargo = request.AplicaRecargo,
            PorcentajeRecargo = request.AplicaRecargo ? request.PorcentajeRecargo : 0,
            ImprimirReciboAlCobrar = request.ImprimirReciboAlCobrar,
            EnviarReciboPorEmail = request.EnviarReciboPorEmail,
            ContratoSeguroObligatorio = request.ContratoSeguroObligatorio,
            ContratoPlazoMeses = request.ContratoPlazoMeses,
            GenerarContratoAlCrearAbono = request.GenerarContratoAlCrearAbono,
            EmailAvisos = request.EmailAvisos.Trim(),
            Activo = true
        };

        _repository.Add(datos);
        _metodoDePagoService.SeedDefaults(datos.EstacionamientoId);
        return datos;
    }

    public DatosEstacionamiento Editar(EditarEstacionamientoRequest request)
    {
        Validar(
            request.Nombre,
            request.Direccion,
            request.EmailAvisos,
            request.DiaVencimientoAbono,
            request.AplicaRecargo,
            request.PorcentajeRecargo,
            request.ContratoPlazoMeses);

        var datos = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        datos.Nombre = request.Nombre.Trim();
        datos.Direccion = request.Direccion.Trim();
        datos.LocadorNombre = string.IsNullOrWhiteSpace(request.LocadorNombre) ? null : request.LocadorNombre.Trim();
        datos.LocadorDocumento = string.IsNullOrWhiteSpace(request.LocadorDocumento) ? null : request.LocadorDocumento.Trim();
        datos.LocadorDomicilio = string.IsNullOrWhiteSpace(request.LocadorDomicilio) ? null : request.LocadorDomicilio.Trim();
        datos.DiaVencimientoAbono = request.DiaVencimientoAbono;
        datos.AplicaRecargo = request.AplicaRecargo;
        datos.PorcentajeRecargo = request.AplicaRecargo ? request.PorcentajeRecargo : 0;
        datos.ImprimirReciboAlCobrar = request.ImprimirReciboAlCobrar;
        datos.EnviarReciboPorEmail = request.EnviarReciboPorEmail;
        datos.ContratoSeguroObligatorio = request.ContratoSeguroObligatorio;
        datos.ContratoPlazoMeses = request.ContratoPlazoMeses;
        datos.GenerarContratoAlCrearAbono = request.GenerarContratoAlCrearAbono;
        datos.EmailAvisos = request.EmailAvisos.Trim();

        _repository.Update(datos);
        return datos;
    }

    public void BajaLogica()
    {
        var datos = _repository.GetById(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        datos.Activo = false;
        _repository.Update(datos);
    }

    private static void Validar(
        string nombre,
        string direccion,
        string emailAvisos,
        int diaVencimiento,
        bool aplicaRecargo,
        decimal porcentajeRecargo,
        int contratoPlazoMeses)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("Debe ingresar un nombre.");

        if (string.IsNullOrWhiteSpace(direccion))
            throw new Exception("Debe ingresar una dirección.");

        if (string.IsNullOrWhiteSpace(emailAvisos))
            throw new Exception("Debe ingresar el email de avisos del estacionamiento.");

        if (emailAvisos.Trim().Length > 200)
            throw new Exception("El email de avisos no puede superar 200 caracteres.");

        if (diaVencimiento < 1 || diaVencimiento > 31)
            throw new Exception("El día de vencimiento debe estar entre 1 y 31.");

        if (aplicaRecargo && porcentajeRecargo <= 0)
            throw new Exception("El porcentaje de recargo debe ser mayor a cero.");

        if (!aplicaRecargo && porcentajeRecargo != 0)
            throw new Exception("No puede ingresar un porcentaje si el recargo está deshabilitado.");

        if (contratoPlazoMeses < 1 || contratoPlazoMeses > 120)
            throw new Exception("El plazo del contrato debe estar entre 1 y 120 meses.");
    }
}
