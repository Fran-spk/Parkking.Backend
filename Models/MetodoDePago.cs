using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parkking_backend.Models;

namespace MODELO
{
    public class MetodoDePago
    {

        private int metododepagoId;
        private string nombreMetodo;
        private bool estado;

        [Required]
        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }
        public int MetodoDePagoId
        {
            get { return metododepagoId; }
            set { metododepagoId = value; }
        }

        public string NombreMetodo
        {
            get { return nombreMetodo; }
            set { nombreMetodo = value; }
        }

        public bool Estado
        {
            get { return estado; }
            set { estado = value; }
        }

        public override string ToString()
        {
            return NombreMetodo.ToString();
        }
    }
}
