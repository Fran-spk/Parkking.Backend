using Mapster;
using Parkking.DTOs.Abonos;
using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Api.Mapping;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Cliente, Parkking.DTOs.Clientes.ClienteDto>.NewConfig()
            .Map(dest => dest.Abonos, src => src.Abonos.Where(a => a.Activo));

        TypeAdapterConfig<AbonoPlaza, AbonoPlazaDto>.NewConfig()
            .Map(dest => dest.Cochera, src => src.Cochera);

        TypeAdapterConfig<AbonoVehiculo, AbonoVehiculoDto>.NewConfig()
            .Map(dest => dest.Patente, src => src.Vehiculo.Patente)
            .Map(dest => dest.ModeloVehiculo, src => src.Vehiculo.ModeloVehiculo)
            .Map(dest => dest.TipoVehiculoId, src => src.Vehiculo.TipoVehiculoId)
            .Map(dest => dest.TipoVehiculo, src => src.Vehiculo.TipoVehiculo)
            .Map(dest => dest.Plaza, src => src.AbonoPlaza != null ? src.AbonoPlaza.Cochera : null);

        TypeAdapterConfig<Abono, AbonoDto>.NewConfig()
            .Map(dest => dest.AbonoId, src => src.AbonoId)
            .Map(dest => dest.Plazas, src => src.Plazas.Where(p => p.Activo).ToList())
            .Map(dest => dest.Vehiculos, src => src.AbonoVehiculos.ToList())
            .Map(dest => dest.CocheraId, src => src.Plazas.Where(p => p.Activo).Select(p => (int?)p.CocheraId).FirstOrDefault())
            .Map(dest => dest.Cochera, src => src.Plazas.Where(p => p.Activo).Select(p => p.Cochera).FirstOrDefault())
            .Map(dest => dest.Patente, src => src.AbonoVehiculos.Select(av => av.Vehiculo.Patente).FirstOrDefault())
            .Map(dest => dest.ModeloVehiculo, src => src.AbonoVehiculos.Select(av => av.Vehiculo.ModeloVehiculo).FirstOrDefault())
            .Map(dest => dest.TipoVehiculoId, src => src.AbonoVehiculos.Select(av => (int?)av.Vehiculo.TipoVehiculoId).FirstOrDefault())
            .Map(dest => dest.TipoVehiculo, src => src.AbonoVehiculos.Select(av => av.Vehiculo.TipoVehiculo).FirstOrDefault());

        TypeAdapterConfig<Abono, AbonoCocheraDto>.NewConfig()
            .Inherits<Abono, AbonoDto>();

        TypeAdapterConfig<Cochera, Parkking.DTOs.Cocheras.CocheraResponseDto>.NewConfig()
            .Map(dest => dest.EstaDisponible, src => src.EstaDisponible())
            .Map(dest => dest.VehiculosPermitidosIds, src => src.VehiculosPermitidos.Select(v => v.TipoVehiculoId).ToList());

        TypeAdapterConfig<MovimientoCaja, MovimientoCajaDto>.NewConfig()
            .Map(dest => dest.Tipo, src => (int)src.Tipo)
            .Map(dest => dest.TipoDescripcion, src => src.Tipo == TipoMovimiento.Ingreso ? "Ingreso" : "Gasto")
            .Map(dest => dest.TipoConcepto, src => (int)src.TipoConcepto)
            .Map(dest => dest.TipoConceptoDescripcion, src => GetTipoConceptoDescripcion(src.TipoConcepto))
            .Map(dest => dest.Usuario, src => src.Usuario.UsuarioName ?? "User no encontrado");
    }

    private static string GetTipoConceptoDescripcion(TipoConcepto tipo) => tipo switch
    {
        TipoConcepto.PagoAbono => "Pago abono",
        TipoConcepto.ReintegroCliente => "Reintegro cliente",
        TipoConcepto.GastoCochera => "Gasto cochera",
        TipoConcepto.CargoCliente => "Cargo cliente",
        _ => "Otro"
    };
}
