namespace Parkking.DTOs.Auth;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public List<string> Grupos { get; set; } = new();
}

public class EstacionamientoUsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class SeleccionarEstacionamientoRequest
{
    public int EstacionamientoId { get; set; }
}
