using Mapster;
using Parkking.DTOs.Abonos;
using Parkking.DTOs.Estacionamiento;
using Parkking.DTOs.Usuario;
using Parkking.Models;
using Parkking.Models.Seguridad;

namespace Parkking.Api.Mapping;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Cliente, Parkking.DTOs.Clientes.ClienteDto>.NewConfig()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Documento, src => src.Documento)
            .Map(dest => dest.Domicilio, src => src.Domicilio)
            .Map(dest => dest.Abonos, src => src.Abonos.Where(a => a.Activo));

        TypeAdapterConfig<DatosEstacionamiento, DatosEstacionamientoDto>.NewConfig()
            .Map(dest => dest.EmailAvisos, src => src.EmailAvisos)
            .Map(dest => dest.LocadorNombre, src => src.LocadorNombre)
            .Map(dest => dest.LocadorDocumento, src => src.LocadorDocumento)
            .Map(dest => dest.LocadorDomicilio, src => src.LocadorDomicilio);

        TypeAdapterConfig<DatosEstacionamiento, EstacionamientoDto>.NewConfig()
            .Inherits<DatosEstacionamiento, DatosEstacionamientoDto>();

        TypeAdapterConfig<Usuario, UsuarioDto>.NewConfig()
            .Map(dest => dest.UsuarioId, src => src.USU_ID)
            .Map(dest => dest.Mail, src => src.Mail)
            .Map(dest => dest.Telefono, src => string.IsNullOrWhiteSpace(src.Telefono) ? null : src.Telefono);

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
            .Map(dest => dest.Email, src => src.Email)
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
            .Map(dest => dest.AbonosActivos, src => src.ContarAbonosActivos())
            .Map(dest => dest.CapacidadMaxima, src => src.CapacidadMaxima())
            .Map(dest => dest.MaxOcupacion, src => src.MultipleOcupacion ? src.CapacidadMaxima() : (int?)null)
            .Map(dest => dest.VehiculosPermitidosIds, src => src.VehiculosPermitidos.Select(v => v.TipoVehiculoId).ToList());
    }
}
