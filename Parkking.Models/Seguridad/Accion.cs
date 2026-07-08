using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Seguridad
{

    public partial class Accion
    {
        public Accion()
        {
            Grupos = new HashSet<Grupo>();
            Usuario_Estacionamiento = new HashSet<UsuarioEstacionamiento>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ACC_ID { get; set; }
        [StringLength(60)]
        public string ACC_NOMBRE { get; set; }
        public virtual ICollection<Grupo> Grupos { get; set; }
        public virtual ICollection<UsuarioEstacionamiento> Usuario_Estacionamiento { get; set; }
    }
}
