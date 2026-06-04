using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELO.seguridad
{
    public partial class Modulo
    {       
        public Modulo()
        {
            Formularios = new HashSet<Formulario>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MOD_ID { get; set; }

        [StringLength(60)]
        public string MOD_NOMBRE { get; set; }

       
        public virtual ICollection<Formulario> Formularios { get; set; }
    }
}
