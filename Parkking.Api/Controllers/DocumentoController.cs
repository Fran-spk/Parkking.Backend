using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkking.DTOs.Documentos;
using Parkking.Services;

namespace Parkking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/documentos")]
public class DocumentoController : ControllerBase
{
    private readonly DocumentoService _service;

    public DocumentoController(DocumentoService service) => _service = service;

    /// <summary>Vista agregada para el detalle de un abono (abono + vehículos + cliente).</summary>
    [HttpGet("abono/{abonoId:int}")]
    public ActionResult<DocumentoAbonoVistaDto> ListarPorAbono(int abonoId)
    {
        try { return Ok(_service.ListarParaAbono(abonoId)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPost]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<ActionResult<DocumentoDto>> Subir(
        [FromForm] IFormFile archivo,
        [FromForm] string tipo,
        [FromForm] int? clienteId,
        [FromForm] int? vehiculoId,
        [FromForm] int? abonoId,
        [FromForm] DateOnly? fechaVencimiento,
        [FromForm] string? observacion,
        CancellationToken ct)
    {
        try
        {
            return Ok(await _service.SubirAsync(
                archivo, tipo, clienteId, vehiculoId, abonoId, fechaVencimiento, observacion, ct));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}/archivo")]
    public async Task<IActionResult> Descargar(int id, CancellationToken ct)
    {
        try
        {
            var (stream, contentType, fileName) = await _service.DescargarAsync(id, ct);
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public ActionResult Eliminar(int id)
    {
        try
        {
            _service.Eliminar(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
