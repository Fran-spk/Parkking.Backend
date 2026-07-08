using Mapster;
using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Models.Seguridad;

namespace Parkking.Api.Mapping;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Cliente, Parkking.DTOs.Clientes.ClienteDto>.NewConfig()
            .Map(dest => dest.Abonos, src => src.Abonos.Where(a => a.Activo));

        TypeAdapterConfig<Cochera, Parkking.DTOs.Cocheras.CocheraResponseDto>.NewConfig()
            .Map(dest => dest.EstaDisponible, src => src.EstaDisponible())
            .Map(dest => dest.VehiculosPermitidosIds, src => src.VehiculosPermitidos.Select(v => v.TipoVehiculoId).ToList());

        TypeAdapterConfig<MovimientoCaja, MovimientoCajaDto>.NewConfig()
            .Map(dest => dest.Tipo, src => (int)src.Tipo)
            .Map(dest => dest.TipoDescripcion, src => src.Tipo == TipoMovimiento.Ingreso ? "Ingreso" : "Gasto")
            .Map(dest => dest.TipoConcepto, src => (int)src.TipoConcepto)
            .Map(dest => dest.TipoConceptoDescripcion, src => GetTipoConceptoDescripcion(src.TipoConcepto))
            .Map(dest => dest.Usuario, src => src.Usuario.USU_USUARIO ?? "User no encontrado");
    }

    private static string GetTipoConceptoDescripcion(TipoConcepto tipo) => tipo switch
    {
        TipoConcepto.PagoMensual => "Pago mensual",
        TipoConcepto.ReintegroCliente => "Reintegro cliente",
        TipoConcepto.GastoCochera => "Gasto cochera",
        TipoConcepto.CargoCliente => "Cargo cliente",
        _ => "Otro"
    };
}
