using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parkking.DTOs.Documentos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;
using Parkking.Services.Documentos;

namespace Parkking.Services;

public class DocumentoService
{
    private readonly DocumentoRepository _repo;
    private readonly IDocumentStorage _storage;
    private readonly IEstacionamientoContext _tenant;
    private readonly DocumentStorageOptions _options;

    public DocumentoService(
        DocumentoRepository repo,
        IDocumentStorage storage,
        IEstacionamientoContext tenant,
        IOptions<DocumentStorageOptions> options)
    {
        _repo = repo;
        _storage = storage;
        _tenant = tenant;
        _options = options.Value;
    }

    private int TenantId => _tenant.EstacionamientoId;

    /// <summary>
    /// Vista agregada para el detalle de abono: docs del abono + vehículos del abono + cliente.
    /// </summary>
    public DocumentoAbonoVistaDto ListarParaAbono(int abonoId)
    {
        var abono = _repo.GetAbonoWithVehiculos(abonoId)
            ?? throw new Exception("Abono no encontrado.");
        if (abono.EstacionamientoId != TenantId)
            throw new Exception("Abono no encontrado.");

        var vehiculoIds = abono.AbonoVehiculos
            .Where(av => av.Vehiculo != null)
            .Select(av => av.VehiculoId)
            .Distinct()
            .ToList();

        var docs = new List<Documento>();
        docs.AddRange(_repo.ListActivosByAbono(abonoId));
        docs.AddRange(_repo.ListActivosByVehiculos(vehiculoIds));
        docs.AddRange(_repo.ListActivosByCliente(abono.ClienteId));

        var mapped = docs
            .GroupBy(d => d.DocumentoId)
            .Select(g => g.First())
            .OrderByDescending(d => d.FechaCarga)
            .Select(Map)
            .ToList();

        return new DocumentoAbonoVistaDto
        {
            AbonoId = abonoId,
            Documentos = mapped,
        };
    }

    public async Task<DocumentoDto> SubirAsync(
        IFormFile archivo,
        string tipo,
        int? clienteId,
        int? vehiculoId,
        int? abonoId,
        DateOnly? fechaVencimiento,
        string? observacion,
        CancellationToken ct = default)
    {
        if (archivo == null || archivo.Length <= 0)
            throw new Exception("Debés seleccionar un archivo.");

        if (archivo.Length > _options.MaxBytes)
            throw new Exception($"El archivo supera el máximo de {_options.MaxBytes / (1024 * 1024)} MB.");

        var contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
            ? "application/octet-stream"
            : archivo.ContentType.Trim();

        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new Exception("Tipo de archivo no permitido. Usá PDF, JPG, PNG o WEBP.");

        var tipoNorm = (tipo ?? "").Trim();
        if (!TipoDocumento.EsValido(tipoNorm))
            throw new Exception("Tipo de documento inválido.");

        ValidarDuenoSegunTipo(tipoNorm, clienteId, vehiculoId, abonoId);
        await ValidarExistenciaDuenosAsync(clienteId, vehiculoId, abonoId);

        await using var stream = archivo.OpenReadStream();
        var (ruta, bytes) = await _storage.SaveAsync(
            TenantId,
            stream,
            contentType,
            archivo.FileName,
            ct);

        var doc = new Documento
        {
            EstacionamientoId = TenantId,
            Tipo = tipoNorm,
            NombreOriginal = Path.GetFileName(archivo.FileName),
            RutaRelativa = ruta,
            ContentType = contentType,
            TamanoBytes = bytes,
            FechaVencimiento = fechaVencimiento,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
            FechaCarga = DateTime.UtcNow,
            Activo = true,
            ClienteId = clienteId,
            VehiculoId = vehiculoId,
            AbonoId = abonoId,
        };

        _repo.Add(doc);
        _repo.SaveChanges();

        // Reload for nav props
        var saved = _repo.GetById(doc.DocumentoId) ?? doc;
        return Map(saved);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DescargarAsync(
        int documentoId,
        CancellationToken ct = default)
    {
        var doc = _repo.GetById(documentoId)
            ?? throw new Exception("Documento no encontrado.");
        if (doc.EstacionamientoId != TenantId)
            throw new Exception("Documento no encontrado.");

        return await _storage.OpenReadAsync(doc.RutaRelativa, doc.ContentType, doc.NombreOriginal, ct);
    }

    public void Eliminar(int documentoId)
    {
        var doc = _repo.GetById(documentoId)
            ?? throw new Exception("Documento no encontrado.");
        if (doc.EstacionamientoId != TenantId)
            throw new Exception("Documento no encontrado.");

        doc.Activo = false;
        _repo.SaveChanges();
        _storage.DeleteIfExists(doc.RutaRelativa);
    }

    private Task ValidarExistenciaDuenosAsync(int? clienteId, int? vehiculoId, int? abonoId)
    {
        if (clienteId is int c)
        {
            var cliente = _repo.GetCliente(c)
                ?? throw new Exception("Cliente no encontrado.");
            if (cliente.EstacionamientoId != TenantId)
                throw new Exception("Cliente no encontrado.");
        }

        if (vehiculoId is int v)
        {
            var vehiculo = _repo.GetVehiculo(v)
                ?? throw new Exception("Vehículo no encontrado.");
            if (vehiculo.EstacionamientoId != TenantId)
                throw new Exception("Vehículo no encontrado.");
        }

        if (abonoId is int a)
        {
            var abono = _repo.GetAbonoWithVehiculos(a)
                ?? throw new Exception("Abono no encontrado.");
            if (abono.EstacionamientoId != TenantId)
                throw new Exception("Abono no encontrado.");
        }

        return Task.CompletedTask;
    }

    private static void ValidarDuenoSegunTipo(string tipo, int? clienteId, int? vehiculoId, int? abonoId)
    {
        var duenos = (clienteId.HasValue ? 1 : 0)
            + (vehiculoId.HasValue ? 1 : 0)
            + (abonoId.HasValue ? 1 : 0);

        if (duenos != 1)
            throw new Exception("El documento debe anclarse a exactamente un dueño: cliente, vehículo o abono.");

        switch (tipo)
        {
            case TipoDocumento.Seguro:
            case TipoDocumento.Cedula:
                if (!vehiculoId.HasValue)
                    throw new Exception($"{tipo} debe asociarse a un vehículo.");
                break;
            case TipoDocumento.Dni:
                if (!clienteId.HasValue)
                    throw new Exception("DNI debe asociarse a un cliente.");
                break;
            case TipoDocumento.ContratoFirmado:
                if (!abonoId.HasValue)
                    throw new Exception("El contrato firmado debe asociarse a un abono.");
                break;
            case TipoDocumento.Otro:
                break;
        }
    }

    private static DocumentoDto Map(Documento d)
    {
        string duenoTipo;
        string grupo;
        if (d.VehiculoId.HasValue)
        {
            duenoTipo = "Vehiculo";
            var patente = d.Vehiculo?.Patente ?? $"#{d.VehiculoId}";
            grupo = $"Vehículo {patente}";
        }
        else if (d.AbonoId.HasValue)
        {
            duenoTipo = "Abono";
            grupo = "Contrato / abono";
        }
        else
        {
            duenoTipo = "Cliente";
            grupo = d.Cliente?.Nombre != null ? $"Cliente · {d.Cliente.Nombre}" : "Cliente";
        }

        return new DocumentoDto
        {
            DocumentoId = d.DocumentoId,
            Tipo = d.Tipo,
            NombreOriginal = d.NombreOriginal,
            ContentType = d.ContentType,
            TamanoBytes = d.TamanoBytes,
            FechaVencimiento = d.FechaVencimiento,
            Observacion = d.Observacion,
            FechaCarga = d.FechaCarga,
            DuenoTipo = duenoTipo,
            ClienteId = d.ClienteId,
            ClienteNombre = d.Cliente?.Nombre,
            VehiculoId = d.VehiculoId,
            VehiculoPatente = d.Vehiculo?.Patente,
            AbonoId = d.AbonoId,
            GrupoLabel = grupo,
        };
    }
}
