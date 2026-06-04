using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELO.seguridad
{
    
    public partial class Grupo
    {

        public Grupo()
        {
            Acciones = new HashSet<Accion>();
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GRU_ID { get; set; }

        [StringLength(60)]
        public string GRU_NOMBRE { get; set; }

        [StringLength(60)]
        public string GRU_DESCRIPCION { get; set; }

        [ForeignKey("Estado_Grupo")]
        public int EST_GRU_ID { get; set; }

        public virtual Estado_Grupo Estado_Grupo { get; set; }

        public virtual ICollection<Accion> Acciones { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }

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
