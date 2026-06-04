using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELO.seguridad
{


    public partial class Formulario
    {       
        public Formulario()
        {
            Acciones = new HashSet<Accion>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FOR_ID { get; set; }

        [StringLength(60)]
        public string FOR_NOMBRE { get; set; }


        [ForeignKey("Modulo")]
        public int? MOD_ID { get; set; }
       
        public virtual ICollection<Accion> Acciones { get; set; }
   
        public virtual Modulo Modulo { get; set; }
    }
}
