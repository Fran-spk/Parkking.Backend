using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Parkking.Services.Documentos;

public class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    /// <summary>Carpeta relativa al ContentRoot (default: App_Data/uploads).</summary>
    public string RootRelativePath { get; set; } = Path.Combine("App_Data", "uploads");

    public long MaxBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } =
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
    };
}

public interface IDocumentStorage
{
    Task<(string RutaRelativa, long Bytes)> SaveAsync(
        int estacionamientoId,
        Stream content,
        string contentType,
        string originalFileName,
        CancellationToken ct = default);

    Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(
        string rutaRelativa,
        string contentType,
        string originalFileName,
        CancellationToken ct = default);

    void DeleteIfExists(string rutaRelativa);
}

public class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _root;
    private readonly DocumentStorageOptions _options;

    public LocalDocumentStorage(IHostEnvironment env, IOptions<DocumentStorageOptions> options)
    {
        _options = options.Value;
        _root = Path.GetFullPath(Path.Combine(env.ContentRootPath, _options.RootRelativePath));
        Directory.CreateDirectory(_root);
    }

    public async Task<(string RutaRelativa, long Bytes)> SaveAsync(
        int estacionamientoId,
        Stream content,
        string contentType,
        string originalFileName,
        CancellationToken ct = default)
    {
        if (content.CanSeek)
            content.Position = 0;

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ExtFromContentType(contentType);

        var safeExt = string.Concat(ext.Where(c => char.IsLetterOrDigit(c) || c == '.')).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeExt) || safeExt[0] != '.')
            safeExt = ".bin";

        var relative = Path.Combine(
            estacionamientoId.ToString(),
            DateTime.UtcNow.ToString("yyyy"),
            DateTime.UtcNow.ToString("MM"),
            $"{Guid.NewGuid():N}{safeExt}");

        var full = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await using var fs = new FileStream(full, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);
        var bytes = fs.Length;

        // Normalizar separadores para DB
        return (relative.Replace('\\', '/'), bytes);
    }

    public Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(
        string rutaRelativa,
        string contentType,
        string originalFileName,
        CancellationToken ct = default)
    {
        var full = ResolveSafePath(rutaRelativa);
        if (!File.Exists(full))
            throw new FileNotFoundException("Archivo no encontrado en almacenamiento.");

        Stream stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult((stream, contentType, originalFileName));
    }

    public void DeleteIfExists(string rutaRelativa)
    {
        try
        {
            var full = ResolveSafePath(rutaRelativa);
            if (File.Exists(full))
                File.Delete(full);
        }
        catch
        {
            // Baja lógica ya ocurrió; no fallar si el archivo no está.
        }
    }

    private string ResolveSafePath(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
            throw new Exception("Ruta de archivo inválida.");

        var combined = Path.GetFullPath(Path.Combine(_root, rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));
        if (!combined.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Ruta de archivo inválida.");

        return combined;
    }

    private static string ExtFromContentType(string contentType) => contentType.ToLowerInvariant() switch
    {
        "application/pdf" => ".pdf",
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".bin"
    };
}
