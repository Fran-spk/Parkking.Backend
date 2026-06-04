using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using Parkking_backend.Services;
using System.Globalization;
using System.Text.Json.Serialization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PagoMensualController : ControllerBase
    {
        private readonly PagoService _service;

        public PagoMensualController(PagoService service)
        {
            _service = service;
        }

        [HttpPost("pagar")]
        public ActionResult RegistrarPago([FromBody] RegistrarPagoRequest request)
        {
            try
            {
                _service.RegistrarPago(request);
                return Ok("Pago registrado exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("sugerido/{abonoId}")]
        public ActionResult GetSugerido(int abonoId, [FromQuery] DateOnly? mes)
        {
            try
            {
                return Ok(_service.GetSugerido(abonoId, mes));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("abono/{id}")]
        public ActionResult GetByAbono(int id)
        {
            return Ok(_service.GetByAbono(id));
        }

        [HttpGet("cliente/{id}")]
        public ActionResult GetByCliente(int id)
        {
            return Ok(_service.GetByCliente(id));
        }

        [HttpGet("deuda/{id}")]
        public ActionResult GetDeuda(int id)
        {
            try
            {
                return Ok(_service.GetDeuda(id));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetAll([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
        {
            return Ok(_service.GetAll(desde, hasta));
        }
    }

    public class PagoSugeridoDto
    {
        public decimal Monto { get; set; }
        public decimal Recargo { get; set; }
        public bool AplicaProporcional { get; set; }
        public bool EsPrecioAcordado { get; set; }
        public DateOnly MesDate { get; set; }
    }

    public class MesImpagoDto
    {
        public int AbonoCocheraId { get; set; }
        public string NumeroCochera { get; set; }
        public string TipoVehiculo { get; set; }
        public string Mes { get; set; }
        public DateOnly MesDate { get; set; }
        public decimal Monto { get; set; }
        public decimal Recargo { get; set; }
        public decimal Total { get; set; }
    }

    public class DeudaClienteDto
    {
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public List<MesImpagoDto> MesesImpagos { get; set; }
        public decimal TotalAdeudado => MesesImpagos?.Sum(m => m.Total) ?? 0;
    }

    public class RegistrarPagoRequest
    {
        public int AbonoCocheraId { get; set; }
        public string Mes { get; set; }
        public decimal Monto { get; set; }
        public decimal? Recargo { get; set; }
        public string? Observacion { get; set; }
        public string? MercadoPagoId { get; set; }
    }
}