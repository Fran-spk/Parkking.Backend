using Parkking.DTOs.Recibos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class ReciboService
{
    private readonly ReciboRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public ReciboService(ReciboRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public ReciboDto GetById(int id)
    {
        var recibo = _repository.GetById(id)
            ?? throw new Exception("Recibo no encontrado.");
        return Map(recibo);
    }

    public ReciboDto GetByPagoId(int pagoId)
    {
        var recibo = _repository.GetByPagoId(pagoId)
            ?? throw new Exception("No hay recibo para ese pago.");
        return Map(recibo);
    }

    public List<ReciboDto> Listar(DateTime? desde, DateTime? hasta, string? cliente, string? q) =>
        _repository.Listar(desde, hasta, cliente, q).Select(Map).ToList();

    /// <summary>Genera el recibo de un pago recién persistido (misma transacción).</summary>
    public Recibo GenerarDesdePago(
        Pago pago,
        Abono abono,
        IEnumerable<(DateOnly Inicio, DateOnly Fin, string Label)> periodos,
        string? metodoPagoLabel = null)
    {
        var existente = _repository.GetByPagoId(pago.PagoId);
        if (existente != null)
            return existente;

        var cocheras = abono.Plazas?
            .Where(p => p.Activo)
            .Select(p => p.Cochera?.Numero)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList() ?? new List<string?>();

        var patentes = abono.AbonoVehiculos?
            .Select(av => av.Vehiculo?.Patente)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList() ?? new List<string?>();

        var periodosLabel = string.Join(", ", periodos.Select(p => p.Label).Where(l => !string.IsNullOrWhiteSpace(l)));

        var recibo = new Recibo
        {
            EstacionamientoId = pago.EstacionamientoId,
            PagoId = pago.PagoId,
            AbonoId = abono.AbonoId,
            Numero = _repository.ProximoNumero(pago.EstacionamientoId),
            FechaEmision = pago.FechaHora,
            Monto = pago.MontoTotal - (pago.Recargo ?? 0),
            Recargo = pago.Recargo ?? 0,
            ClienteNombre = abono.Cliente?.Nombre ?? "Sin cliente",
            CocherasLabel = cocheras.Count > 0 ? string.Join(", ", cocheras) : null,
            PatentesLabel = patentes.Count > 0 ? string.Join(", ", patentes) : null,
            PeriodosLabel = string.IsNullOrWhiteSpace(periodosLabel) ? null : periodosLabel,
            Cobrador = abono.Cobrador,
            MetodoPagoLabel = string.IsNullOrWhiteSpace(metodoPagoLabel)
                ? pago.MetodoDePago?.Nombre
                : metodoPagoLabel.Trim(),
            Observacion = pago.Observacion,
            Anulado = false
        };

        _repository.Add(recibo);
        _repository.SaveChanges();
        return recibo;
    }

    public ReciboDto Anular(int id, string? motivo)
    {
        var recibo = _repository.GetById(id)
            ?? throw new Exception("Recibo no encontrado.");

        if (recibo.Anulado)
            throw new Exception("El recibo ya está anulado.");

        recibo.Anulado = true;
        recibo.MotivoAnulacion = string.IsNullOrWhiteSpace(motivo)
            ? "Anulado"
            : motivo.Trim();
        _repository.SaveChanges();
        return Map(recibo);
    }

    private static ReciboDto Map(Recibo r) => new()
    {
        ReciboId = r.ReciboId,
        Numero = r.Numero,
        NumeroFormateado = r.NumeroFormateado,
        PagoId = r.PagoId,
        AbonoId = r.AbonoId,
        FechaEmision = r.FechaEmision,
        Monto = r.Monto,
        Recargo = r.Recargo,
        Total = r.Total,
        ClienteNombre = r.ClienteNombre,
        CocherasLabel = r.CocherasLabel,
        PatentesLabel = r.PatentesLabel,
        PeriodosLabel = r.PeriodosLabel,
        Cobrador = r.Cobrador,
        MetodoPagoLabel = r.MetodoPagoLabel,
        Observacion = r.Observacion,
        Anulado = r.Anulado,
        MotivoAnulacion = r.MotivoAnulacion
    };
}
