using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkking.DTOs.Estacionamiento
{
    public class EditarEstacionamientoRequest
    {
        public string Nombre { get; set; }

        public string Direccion { get; set; }

        public int DiaVencimientoAbono { get; set; }

        public bool AplicaRecargo { get; set; }

        public decimal PorcentajeRecargo { get; set; }

        public int? DiasUmbralProporcional { get; set; }
    }
}
