namespace Parkking.DTOs.MetodosDePago;

public class MetodoDePagoDto
{
    public int MetodoDePagoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CrearMetodoDePagoRequest
{
    public string Nombre { get; set; } = string.Empty;
}

public class EditarMetodoDePagoRequest
{
    public string Nombre { get; set; } = string.Empty;
}
