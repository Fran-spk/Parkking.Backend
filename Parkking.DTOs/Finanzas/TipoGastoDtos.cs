namespace Parkking.DTOs.Finanzas;

public class TipoGastoDto
{
    public int TipoGastoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CrearTipoGastoRequest
{
    public string Nombre { get; set; } = string.Empty;
}

public class EditarTipoGastoRequest
{
    public string Nombre { get; set; } = string.Empty;
}
