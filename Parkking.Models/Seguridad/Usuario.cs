using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Parkking.Models.Enums;
namespace Parkking.Models.Seguridad
{
    public partial class Usuario
    {
        public Usuario()
        {
            Usuario_Estacionamiento = new HashSet<UsuarioEstacionamiento>();
        }

        [Key]
        public int USU_ID { get; set; }

        [StringLength(60)]
        public string USU_USUARIO { get; set; }

        [StringLength(64)]
        public string USU_CLAVE { get; set; }

        [StringLength(60)]
        public string USU_MAIL { get; set; }

        public EstadoUsuario? Estado_Usuario { get; set; }

        // Colección hacia la tabla intermedia multi-tenant
        public virtual ICollection<UsuarioEstacionamiento> Usuario_Estacionamiento { get; set; }

        #region Métodos de Conveniencia para Multi-Tenant


        public ReadOnlyCollection<Grupo> GetAllGruposActivos(int estacionamientoId)
        {
            var perfilTenant = Usuario_Estacionamiento
                .FirstOrDefault(ue => ue.ESTACIONAMIENTO_ID == estacionamientoId && ue.Activo);

            if (perfilTenant == null) return new List<Grupo>().AsReadOnly();

            return perfilTenant.Grupos
                .Where(x => x.EstadoGrupo==EstadoGrupo.Habilitado)
                .ToList()
                .AsReadOnly();
        }

        public ReadOnlyCollection<Accion> GetAllAccionesPorEstacionamiento(int estacionamientoId)
        {
            var perfilTenant = Usuario_Estacionamiento
                .FirstOrDefault(ue => ue.ESTACIONAMIENTO_ID == estacionamientoId && ue.Activo);

            if (perfilTenant == null) return new List<Accion>().AsReadOnly();

            // Retorna las acciones asignadas directamente en ese tenant
            return perfilTenant.Acciones.ToList().AsReadOnly();
        }

        #endregion
    }
}
