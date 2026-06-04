using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.DTOs.Shared;
namespace Parkking_backend.DTOs.Abonos;

public static class AbonoCocheraMapper
{
    public static IQueryable<AbonoCochera> WithIncludes(IQueryable<AbonoCochera> query)
    {
        return query
            .Include(a => a.Cliente)
            .Include(a => a.Cochera)
                .ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.TipoVehiculo);
    }


    public static CocheraResumenDto ToCocheraResumen(Cochera cochera)
    {
        return new CocheraResumenDto
        {
            CocheraId = cochera.CocheraId,
            Numero = cochera.Numero,
            CategoriaCocheraId = cochera.CategoriaCocheraId,
            CategoriaCochera = cochera.CategoriaCochera != null
                ? new CategoriaCocheraResumenDto
                {
                    CategoriaCocheraId = cochera.CategoriaCochera.CategoriaCocheraId,
                    Nombre = cochera.CategoriaCochera.Nombre
                }
                : null
        };
    }

    public static AbonoCochera? LoadWithIncludes(EstacionamientoContext context, int abonoCocheraId)
    {
        return WithIncludes(context.AbonoCocheras)
            .FirstOrDefault(a => a.AbonoCocheraId == abonoCocheraId);
    }
}
