using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Enums;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

/// <summary>
/// Altas manuales. El cargo no genera movimiento.
/// El ajuste y el reintegro se registran a través de MovimientoService.
/// </summary>
public class OperacionFinancieraService
{
    private readonly FinanzasMovimientoRepository _movimientos;
    private readonly MovimientoService _movimientoService;
    private readonly GrupoFinancieroRepository _grupos;
    private readonly ReglaAsignacionRepository _reglas;
    private readonly IEstacionamientoContext _estacionamiento;

    public OperacionFinancieraService(
        FinanzasMovimientoRepository movimientos,
        MovimientoService movimientoService,
        GrupoFinancieroRepository grupos,
        ReglaAsignacionRepository reglas,
        IEstacionamientoContext estacionamiento)
    {
        _movimientos = movimientos;
        _movimientoService = movimientoService;
        _grupos = grupos;
        _reglas = reglas;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public OperacionFinancieraDto CrearCargo(CrearCargoRequest request)
    {
        if (request.Importe <= 0)
            throw new Exception("El importe del cargo debe ser mayor a cero.");
        var concepto = RequerirTexto(request.Concepto, "Debe ingresar un concepto.");
        RequireCliente(request.ClienteId);
        if (request.AbonoId <= 0 || !_reglas.ExisteAbono(request.AbonoId, TenantId))
            throw new Exception("El abono no existe en este estacionamiento.");

        var cargo = new Cargo
        {
            EstacionamientoId = TenantId,
            ClienteId = request.ClienteId,
            AbonoId = request.AbonoId,
            Importe = request.Importe,
            Concepto = concepto,
            FechaHora = DateTime.UtcNow
        };
        _movimientos.AddCargo(cargo);
        _movimientos.SaveChanges();

        return new OperacionFinancieraDto
        {
            CargoId = cargo.CargoId,
            Tipo = "Cargo",
            Importe = cargo.Importe,
            ClienteId = cargo.ClienteId
        };
    }

    public List<CargoDto> CargosDeAbono(int abonoId)
    {
        if (!_reglas.ExisteAbono(abonoId, TenantId))
            throw new Exception("El abono no existe en este estacionamiento.");

        return _movimientos.CargosPorAbono(abonoId, TenantId)
            .Select(c => new CargoDto
            {
                CargoId = c.CargoId,
                ClienteId = c.ClienteId,
                AbonoId = c.AbonoId,
                Importe = c.Importe,
                Concepto = c.Concepto,
                FechaHora = c.FechaHora
            })
            .ToList();
    }

    public List<ReintegroDto> ReintegrosDeAbono(int abonoId)
    {
        if (!_reglas.ExisteAbono(abonoId, TenantId))
            throw new Exception("El abono no existe en este estacionamiento.");

        return _movimientos.ReintegrosPorAbono(abonoId, TenantId)
            .Select(r => new ReintegroDto
            {
                ReintegroId = r.ReintegroId,
                ClienteId = r.ClienteId,
                AbonoId = r.Movimiento.AbonoId,
                MovimientoId = r.MovimientoId,
                Importe = r.Importe,
                Beneficiario = r.Beneficiario,
                Motivo = r.Motivo,
                Medio = r.Medio,
                FechaHora = r.FechaHora
            })
            .ToList();
    }

    public OperacionFinancieraDto CrearAjuste(CrearAjusteRequest request)
    {
        if (request.Importe == 0)
            throw new Exception("El importe del ajuste no puede ser cero.");
        var motivo = RequerirTexto(request.Motivo, "Debe ingresar un motivo.");
        if (request.ClienteId is int clienteId)
            RequireCliente(clienteId);
        if (request.AbonoId is int abonoId && !_reglas.ExisteAbono(abonoId, TenantId))
            throw new Exception("El abono no existe en este estacionamiento.");
        if (request.TipoGastoId is int tipoGastoId && !_reglas.ExisteTipoGasto(tipoGastoId, TenantId))
            throw new Exception("El tipo de gasto no existe o está dado de baja.");

        var magnitud = Math.Abs(request.Importe);
        var aFavor = request.AFavorDelCliente || request.Importe < 0;
        var importe = aFavor ? -magnitud : magnitud;
        var movimiento = NuevoMovimiento(TipoMovimiento.Ajuste, importe, motivo, request.ClienteId, request.AbonoId, request.TipoGastoId);
        var grupos = GruposMarcados(request.GrupoFinancieroIds);
        _movimientoService.Registrar(movimiento, grupos, "Alta de ajuste");
        var ajuste = new AjusteFinanciero
        {
            EstacionamientoId = TenantId,
            ClienteId = request.ClienteId,
            Movimiento = movimiento,
            Importe = importe,
            Motivo = motivo,
            FechaHora = movimiento.FechaHora
        };
        _movimientos.AddAjuste(ajuste);
        _movimientos.SaveChanges();

        var dto = MapMovimiento("Ajuste", movimiento);
        dto.AjusteFinancieroId = ajuste.AjusteFinancieroId;
        return dto;
    }

    public OperacionFinancieraDto CrearReintegro(CrearReintegroRequest request)
    {
        if (request.Importe <= 0)
            throw new Exception("El importe del reintegro debe ser mayor a cero.");
        var beneficiario = RequerirTexto(request.Beneficiario, "Debe ingresar el beneficiario.");
        var motivo = RequerirTexto(request.Motivo, "Debe ingresar un motivo.");
        var medio = RequerirTexto(request.Medio, "Debe ingresar el medio de devolución.");
        if (request.ClienteId is int clienteId)
            RequireCliente(clienteId);
        if (request.AbonoId is int abonoId && !_reglas.ExisteAbono(abonoId, TenantId))
            throw new Exception("El abono no existe en este estacionamiento.");
        if (request.TipoGastoId is int tipoGastoId && !_reglas.ExisteTipoGasto(tipoGastoId, TenantId))
            throw new Exception("El tipo de gasto no existe o está dado de baja.");

        var movimiento = NuevoMovimiento(TipoMovimiento.Reintegro, -request.Importe, motivo, request.ClienteId, request.AbonoId, request.TipoGastoId);
        var grupos = GruposMarcados(request.GrupoFinancieroIds);
        _movimientoService.Registrar(movimiento, grupos, "Alta de reintegro");
        var reintegro = new Reintegro
        {
            EstacionamientoId = TenantId,
            ClienteId = request.ClienteId,
            Movimiento = movimiento,
            Beneficiario = beneficiario,
            Importe = request.Importe,
            Motivo = motivo,
            Medio = medio,
            FechaHora = movimiento.FechaHora
        };
        _movimientos.AddReintegro(reintegro);
        _movimientos.SaveChanges();

        var dto = MapMovimiento("Reintegro", movimiento);
        return dto;
    }

    public OperacionFinancieraDto CrearGasto(CrearGastoRequest request)
    {
        if (request.Importe <= 0)
            throw new Exception("El importe del gasto debe ser mayor a cero.");
        var concepto = RequerirTexto(request.Concepto, "Debe ingresar un concepto.");
        if (request.TipoGastoId <= 0 || !_reglas.ExisteTipoGasto(request.TipoGastoId, TenantId))
            throw new Exception("El tipo de gasto no existe o está dado de baja.");

        var movimiento = NuevoMovimiento(TipoMovimiento.Egreso, -request.Importe, concepto, null, null, request.TipoGastoId);
        var grupos = GruposMarcados(request.GrupoFinancieroIds);
        _movimientoService.Registrar(movimiento, grupos, "Alta de gasto");
        var gasto = new Gasto
        {
            EstacionamientoId = TenantId,
            TipoGastoId = request.TipoGastoId,
            Movimiento = movimiento,
            Importe = request.Importe,
            Observacion = concepto,
            FechaHora = movimiento.FechaHora
        };
        _movimientos.AddGasto(gasto);
        _movimientos.SaveChanges();
        return MapMovimiento("Gasto", movimiento);
    }

    private List<int> GruposMarcados(IEnumerable<int>? marcados)
    {
        var ids = new List<int>();
        foreach (var id in marcados ?? Array.Empty<int>())
        {
            var grupo = _grupos.GetById(id, TenantId)
                ?? throw new Exception("Grupo financiero no encontrado.");
            if (!grupo.Activo)
                throw new Exception($"El grupo {grupo.Nombre} está dado de baja.");
            ids.Add(id);
        }
        return ids;
    }

    private Movimiento NuevoMovimiento(
        TipoMovimiento tipo,
        decimal importe,
        string concepto,
        int? clienteId,
        int? abonoId,
        int? tipoGastoId) =>
        new()
        {
            EstacionamientoId = TenantId,
            ClienteId = clienteId,
            AbonoId = abonoId,
            TipoGastoId = tipoGastoId,
            Importe = importe,
            Tipo = tipo,
            Concepto = concepto,
            FechaHora = DateTime.UtcNow,
            UsuarioId = _estacionamiento.GetUsuarioActual().Id
        };

    private void RequireCliente(int clienteId)
    {
        if (!_movimientos.ExisteCliente(clienteId, TenantId))
            throw new Exception("El cliente no existe o está dado de baja.");
    }

    private static string RequerirTexto(string? valor, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new Exception(mensaje);
        var texto = valor.Trim();
        if (texto.Length > 255)
            throw new Exception("El texto no puede superar 255 caracteres.");
        return texto;
    }

    private static OperacionFinancieraDto MapMovimiento(string tipo, Movimiento movimiento) =>
        new()
        {
            MovimientoId = movimiento.MovimientoId,
            Tipo = tipo,
            Importe = movimiento.Importe,
            ClienteId = movimiento.ClienteId,
            GrupoFinancieroIds = movimiento.Grupos.Select(g => g.GrupoFinancieroId).ToList()
        };
}
