namespace Parkking.Models;

public static class TipoMensaje
{
    public const string Recibo = "Recibo";
    public const string Reporte = "Reporte";
    /// <summary>Aviso de actualización de tarifas (futuro).</summary>
    public const string ActualizacionTarifas = "ActualizacionTarifas";
}

public static class EstadoMensaje
{
    public const string Enviado = "Enviado";
    public const string Error = "Error";
    /// <summary>SMTP deshabilitado: se registró el intento sin enviar de verdad.</summary>
    public const string Simulado = "Simulado";
}
