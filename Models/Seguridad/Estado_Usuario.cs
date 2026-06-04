
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELO.seguridad
{


    public partial class Estado_Usuario
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EST_USU_ID { get; set; }

        [StringLength(60)]
        public string EST_USU_NOMBRE { get; set; }
        

        public override string ToString()
        {
            return EST_USU_NOMBRE.ToString();
        }
    }
}
