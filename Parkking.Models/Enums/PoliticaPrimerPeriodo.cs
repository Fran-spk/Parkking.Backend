namespace Parkking.Models.Enums;



/// <summary>

/// Tratamiento del mes de alta al crear el abono.

/// </summary>

public enum PoliticaPrimerPeriodo

{

    /// <summary>Cobrar proporcional el mes de ingreso (solo si día ≥ 3).</summary>

    Prorratear = 0,



    /// <summary>

    /// No cobrar el mes entrante (el de ingreso).

    /// En quincenal: omite la quincena entrante; en anual: omite el año-ciclo entrante.

    /// </summary>

    OmitirMesEntrante = 1,



    /// <summary>Cobrar el mes/slot de ingreso al 100% (sin prorrateo).</summary>

    PeriodoCompleto = 2,

}


