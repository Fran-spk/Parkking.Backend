using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELO.seguridad
{

    public partial class Accion
    {
        public Accion()
        {
            Grupos = new HashSet<Grupo>();
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ACC_ID { get; set; }

        [StringLength(60)]
        public string ACC_NOMBRE { get; set; }

        [ForeignKey("Formulario")]
        public int? FOR_ID { get; set; }

        public virtual Formulario Formulario { get; set; }

        public virtual ICollection<Grupo> Grupos { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
