namespace Parkking.Models;

/// <summary>Tipos de archivo adjunto del dominio contractual.</summary>
public static class TipoDocumento
{
    /// <summary>Póliza de seguro — dueño: vehículo.</summary>
    public const string Seguro = "Seguro";

    /// <summary>Cédula verde / identificación del vehículo — dueño: vehículo.</summary>
    public const string Cedula = "Cedula";

    /// <summary>DNI u otro documento de identidad — dueño: cliente.</summary>
    public const string Dni = "Dni";

    /// <summary>Contrato firmado (escaneo) — dueño: abono. Distinto del PDF regenerable.</summary>
    public const string ContratoFirmado = "ContratoFirmado";

    /// <summary>Otro archivo — dueño: cliente, vehículo o abono.</summary>
    public const string Otro = "Otro";

    public static readonly HashSet<string> Todos = new(StringComparer.OrdinalIgnoreCase)
    {
        Seguro, Cedula, Dni, ContratoFirmado, Otro
    };

    public static bool EsValido(string? tipo) =>
        !string.IsNullOrWhiteSpace(tipo) && Todos.Contains(tipo.Trim());
}
