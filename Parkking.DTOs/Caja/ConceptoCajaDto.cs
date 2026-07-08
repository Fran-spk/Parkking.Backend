using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parkking.Models.Enums;

namespace Parkking.DTOs.Caja
{
    public class ConceptoCajaDto
    {
        public TipoConcepto TipoConcepto { get; set; }

        public string TipoConceptoDescripcion { get; set; } = string.Empty;

        public int CantidadMovimientos { get; set; }

        public decimal Total { get; set; }
    }
}
