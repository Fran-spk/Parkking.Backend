using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models.Seguridad
{
    
    public partial class Grupo
    {

        public Grupo()
        {
            Acciones = new HashSet<Accion>();
            Usuario_Estacionamiento = new HashSet<UsuarioEstacionamiento>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GRU_ID { get; set; }

        [StringLength(60)]
        public string GRU_NOMBRE { get; set; }

        [StringLength(60)]
        public string GRU_DESCRIPCION { get; set; }

        public virtual EstadoGrupo EstadoGrupo { get; set; }

        public virtual ICollection<Accion> Acciones { get; set; }

        public virtual ICollection<UsuarioEstacionamiento> Usuario_Estacionamiento { get; set; }

        public override string ToString()
        {
            return GRU_NOMBRE;
        }


        public bool AgregarAccion(Accion accion)
        {
            var accionExistente = Acciones.FirstOrDefault(x=>x.ACC_ID == accion.ACC_ID);
            if(accionExistente == null)
            {
                Acciones.Add(accion);
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool QuitarAccion(Accion accion)
        {
            var accionExistente = Acciones.FirstOrDefault(x => x.ACC_ID == accion.ACC_ID);
            if (accionExistente != null)
            {
                Acciones.Remove(accion);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
