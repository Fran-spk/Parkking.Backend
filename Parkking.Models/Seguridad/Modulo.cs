using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models.Seguridad
{
    public partial class Modulo
    {       
        public Modulo()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MOD_ID { get; set; }

        [StringLength(60)]
        public string MOD_NOMBRE { get; set; }

    }
}
