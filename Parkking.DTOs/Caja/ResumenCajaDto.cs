using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkking.DTOs.Caja
{
    public class ResumenCajaDto
    {
        public int CajaMensualId { get; set; }
        public DateOnly Mes { get; set; }
        public bool Cerrada { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Saldo { get; set; }

        // Este lo calculo manualmente
        public int CantidadMovimientos { get; set; }
    }
}
