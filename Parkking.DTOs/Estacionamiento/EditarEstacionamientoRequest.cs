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

        public string? LocadorNombre { get; set; }

        public string? LocadorDocumento { get; set; }

        public string? LocadorDomicilio { get; set; }

        public int DiaVencimientoAbono { get; set; }

        public bool AplicaRecargo { get; set; }

        public decimal PorcentajeRecargo { get; set; }

        public bool ImprimirReciboAlCobrar { get; set; } = true;

        /// <summary>Si true, al cobrar se envía el recibo por email al cliente.</summary>
        public bool EnviarReciboPorEmail { get; set; }

        /// <summary>Cláusula de seguro obligatorio en el PDF de contrato.</summary>
        public bool ContratoSeguroObligatorio { get; set; } = true;

        /// <summary>Plazo del contrato en meses (solo PDF).</summary>
        public int ContratoPlazoMeses { get; set; } = 12;

        /// <summary>Si true, al crear un abono se descarga automáticamente el PDF de contrato.</summary>
        public bool GenerarContratoAlCrearAbono { get; set; } = true;

        /// <summary>Email de avisos del estacionamiento (obligatorio).</summary>
        public string EmailAvisos { get; set; } = string.Empty;
    }
}
