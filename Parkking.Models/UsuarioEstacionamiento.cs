using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Seguridad;

namespace Parkking.Models;

public class UsuarioEstacionamiento
{
    public UsuarioEstacionamiento()
    {
        Acciones = new HashSet<Accion>();
        Grupos = new HashSet<Grupo>();
    }

    [Key]
    public int UsuarioEstacionamientoId { get; set; }

    [ForeignKey(nameof(Usuario))]
    public int USU_ID { get; set; }

    public virtual Usuario Usuario { get; set; }

    [ForeignKey(nameof(Estacionamiento))]
    public int ESTACIONAMIENTO_ID { get; set; }

    public virtual DatosEstacionamiento Estacionamiento { get; set; }

    public DateTime FechaAlta { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Accion> Acciones { get; set; }

    public virtual ICollection<Grupo> Grupos { get; set; }

    public bool AgregarGrupo(Grupo grupo)
    {
        var grupoExistente = Grupos.FirstOrDefault(x => x.GRU_ID == grupo.GRU_ID);
        if (grupoExistente != null) return false;

        var accionesPersonalizadas = Acciones.Where(x => grupo.Acciones.Contains(x)).ToList();
        foreach (var accion in accionesPersonalizadas)
            Acciones.Remove(accion);

        Grupos.Add(grupo);
        return true;
    }

    public bool AgregarAccion(Accion accion)
    {
        var ac = Acciones.FirstOrDefault(x => x.ACC_ID == accion.ACC_ID);
        if (ac != null) return false;

        var accionGrupo = Grupos.FirstOrDefault(x => x.Acciones.Contains(accion));
        if (accionGrupo != null) return false;

        Acciones.Add(accion);
        return true;
    }

    public bool QuitarAccion(Accion accion)
    {
        var ac = Acciones.FirstOrDefault(x => x.ACC_ID == accion.ACC_ID);
        if (ac == null) return false;

        Acciones.Remove(accion);
        return true;
    }

    public ReadOnlyCollection<Grupo> getAllGruposActivos()
    {
        return Grupos.Where(x => x.EstadoGrupo == Enums.EstadoGrupo.Habilitado).ToList().AsReadOnly();
    }

    public ReadOnlyCollection<Accion> getAllAcciones()
    {
        return Acciones.ToList().AsReadOnly();
    }
}
