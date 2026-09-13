using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Pagos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagoController : ControllerBase
{
    private readonly PagoService _service;

    public PagoController(PagoService service) => _service = service;

    [HttpPost("pagar")]
    public ActionResult RegistrarPago([FromBody] RegistrarPagoRequest request)
    {
        try { return Ok(_service.RegistrarPago(request)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Cola de cobro: todos los períodos con saldo de abonos activos.</summary>
    [HttpGet("pendientes")]
    public ActionResult GetPendientes() => Ok(_service.GetPendientes());

    [HttpGet("sugerido/{abonoId}")]
    public ActionResult GetSugerido(int abonoId, [FromQuery] DateOnly? mes)
    {
        try { return Ok(_service.GetSugerido(abonoId, mes)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Desglose de liquidación persistido de una cuota.</summary>
    [HttpGet("cuota/{cuotaId:int}/detalles")]
    public ActionResult GetDetallesCuota(int cuotaId)
    {
        try { return Ok(_service.GetDetallesCuota(cuotaId)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    /// <summary>Monto base del período: precio acordado o suma de tarifas de lista (con detalle).</summary>
    [HttpGet("abono/{id}/monto-periodo")]
    public ActionResult GetMontoPeriodo(int id, [FromQuery] DateOnly? periodoInicio = null)
    {
        try { return Ok(_service.GetMontoPeriodo(id, periodoInicio)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>Períodos / cuotas a pagar del abono (con saldo).</summary>
    [HttpGet("abono/{id}/cuotas-pendientes")]
    public ActionResult GetCuotasPendientes(int id)
    {
        try { return Ok(_service.GetCuotasPendientes(id)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    /// <summary>Timeline de períodos del abono (pendiente / parcial / pagada / futuro).</summary>
    [HttpGet("abono/{id}/cuotas")]
    public ActionResult GetCuotas(int id)
    {
        try { return Ok(_service.GetCuotasTimeline(id)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    /// <summary>Historial de pagos del abono, cada uno con sus detalles.</summary>
    [HttpGet("abono/{id}")]
    public ActionResult GetByAbono(int id) => Ok(_service.GetByAbono(id));

    /// <summary>Un pago con el desglose por cuota.</summary>
    [HttpGet("{pagoId:int}/detalles")]
    public ActionResult GetPagoDetalles(int pagoId)
    {
        try { return Ok(_service.GetPagoConDetalles(pagoId)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet("cliente/{id}")]
    public ActionResult GetByCliente(int id) => Ok(_service.GetByCliente(id));

    [HttpGet("deuda/cliente/{id}")]
    public ActionResult GetDeuda(int id)
    {
        try { return Ok(_service.GetDeuda(id)); }
        catch (Exception ex) { return NotFound(ex.Message); }
    }

    [HttpGet]
    public ActionResult GetAll([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta) =>
        Ok(_service.GetAll(desde, hasta));
}
