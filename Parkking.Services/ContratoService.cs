using System.Globalization;
using System.Text;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Parkking.Services;

/// <summary>
/// Genera el PDF de contrato de locación de cochera a partir del abono + config del estacionamiento.
/// El abono es la fuente de verdad; el PDF es un snapshot imprimible.
/// </summary>
public class ContratoService
{
    private readonly AbonoRepository _abonos;
    private readonly IEstacionamientoContext _tenant;

    static ContratoService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ContratoService(AbonoRepository abonos, IEstacionamientoContext tenant)
    {
        _abonos = abonos;
        _tenant = tenant;
    }

    private int TenantId => _tenant.EstacionamientoId;

    public (byte[] Bytes, string FileName) GenerarPdf(int abonoId)
    {
        var abono = _abonos.LoadWithIncludes(abonoId)
            ?? throw new Exception("Abono no encontrado.");
        if (abono.EstacionamientoId != TenantId)
            throw new Exception("Abono no encontrado.");

        var datos = _abonos.GetEstacionamiento(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        var cultura = new CultureInfo("es-AR");
        var model = ArmarModelo(abono, datos, cultura);
        var bytes = Document.Create(container => BuildDocument(container, model)).GeneratePdf();
        var fileName = $"contrato-abono-{abono.AbonoId}.pdf";
        return (bytes, fileName);
    }

    private static ContratoPdfModel ArmarModelo(Abono abono, DatosEstacionamiento datos, CultureInfo cultura)
    {
        var cliente = abono.Cliente;
        var plazas = abono.Plazas.Where(p => p.Activo).Select(p => p.Cochera?.Numero).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()!;
        var vehiculos = abono.AbonoVehiculos
            .Select(av => av.Vehiculo)
            .Where(v => v != null)
            .Select(v => $"{v!.Patente}{(string.IsNullOrWhiteSpace(v.ModeloVehiculo) ? "" : $" ({v.ModeloVehiculo})")}")
            .ToList();

        var plazoMeses = datos.ContratoPlazoMeses > 0 ? datos.ContratoPlazoMeses : 12;
        var inicio = abono.FechaInicio;
        var fin = inicio.AddMonths(plazoMeses);

        var precio = abono.PrecioAcordado;
        var precioTexto = precio.HasValue
            ? precio.Value.ToString("C", cultura)
            : "según tarifa vigente del estacionamiento";

        return new ContratoPdfModel
        {
            EstacionamientoNombre = datos.Nombre,
            InmuebleDireccion = string.IsNullOrWhiteSpace(datos.Direccion) ? "—" : datos.Direccion.Trim(),
            LocadorNombre = PrimeroNoVacio(datos.LocadorNombre, datos.Nombre),
            LocadorDocumento = string.IsNullOrWhiteSpace(datos.LocadorDocumento) ? "—" : datos.LocadorDocumento.Trim(),
            LocadorDomicilio = PrimeroNoVacio(datos.LocadorDomicilio, datos.Direccion),
            LocatarioNombre = cliente?.Nombre?.Trim() ?? "—",
            LocatarioDocumento = string.IsNullOrWhiteSpace(cliente?.Documento) ? "—" : cliente!.Documento!.Trim(),
            LocatarioDomicilio = string.IsNullOrWhiteSpace(cliente?.Domicilio) ? "—" : cliente!.Domicilio!.Trim(),
            PlazasLabel = plazas.Count == 0 ? "—" : string.Join(", ", plazas),
            CantidadPlazas = Math.Max(plazas.Count, 1),
            VehiculosLabel = vehiculos.Count == 0 ? "a designar" : string.Join(", ", vehiculos),
            FechaInicio = inicio,
            FechaFin = fin,
            PlazoMeses = plazoMeses,
            PrecioTexto = precioTexto,
            PeriodicidadLabel = LabelPeriodicidad(abono.PeriodicidadCobro),
            DiaVencimiento = datos.DiaVencimientoAbono,
            AplicaRecargo = datos.AplicaRecargo,
            PorcentajeRecargo = datos.PorcentajeRecargo,
            SeguroObligatorio = datos.ContratoSeguroObligatorio,
            FechaFirma = DateOnly.FromDateTime(DateTime.Today),
            AbonoId = abono.AbonoId,
        };
    }

    private static void BuildDocument(IDocumentContainer container, ContratoPdfModel m)
    {
        // Formato tipo contrato de locación AR: A4, márgenes ~2,5 cm, tipografía serif,
        // texto negro, justificado, cláusulas "PRIMERA.-" en negrita e interlineado amplio.
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(70);
            page.MarginVertical(65);
            page.DefaultTextStyle(x => x
                .FontFamily("Times New Roman")
                .FontSize(11)
                .FontColor(Colors.Black)
                .LineHeight(1.45f));

            page.Header().AlignCenter().Column(col =>
            {
                col.Item().Text("CONTRATO DE LOCACIÓN DE ESPACIO DE ESTACIONAMIENTO")
                    .Bold().FontSize(13).LetterSpacing(0.4f);
                col.Item().PaddingTop(6).Text(m.EstacionamientoNombre)
                    .FontSize(10);
                col.Item().PaddingTop(10).LineHorizontal(0.75f).LineColor(Colors.Black);
                col.Item().PaddingBottom(14);
            });

            page.Content().Column(col =>
            {
                col.Spacing(11);

                col.Item().Text(text =>
                {
                    text.Justify();
                    text.Span("Entre ");
                    text.Span(m.LocadorNombre).Bold();
                    text.Span($", DNI/CUIT {m.LocadorDocumento}, con domicilio en {m.LocadorDomicilio}, en adelante ");
                    text.Span("EL LOCADOR").Bold();
                    text.Span("; y ");
                    text.Span(m.LocatarioNombre).Bold();
                    text.Span($", DNI {m.LocatarioDocumento}, con domicilio en {m.LocatarioDomicilio}, en adelante ");
                    text.Span("EL LOCATARIO").Bold();
                    text.Span("; se celebra el presente contrato de locación de espacio(s) de estacionamiento, sujeto a las siguientes cláusulas:");
                });

                Clausula(col, "PRIMERA",
                    $"EL LOCADOR da en locación a EL LOCATARIO {NumeroEnLetras(m.CantidadPlazas)} ({m.CantidadPlazas}) espacio(s) de estacionamiento fijo(s), identificado(s) con el/los número(s) interno(s) {m.PlazasLabel}, ubicado(s) en el inmueble sito en {m.InmuebleDireccion}, destinado exclusivamente al estacionamiento de vehículo(s) automotor(es). Los espacios se encuentran delimitados y asignados mediante numeración interna del estacionamiento.");

                var segunda = new StringBuilder();
                segunda.Append("El espacio se destina exclusivamente al estacionamiento del/los vehículo(s) de EL LOCATARIO. Queda prohibido: a) el depósito de mercaderías o materiales inflamables o peligrosos; b) realizar reparaciones mecánicas en el lugar; c) ceder, subarrendar o transferir el uso del espacio sin autorización escrita de EL LOCADOR.");
                if (!string.IsNullOrWhiteSpace(m.VehiculosLabel) && m.VehiculosLabel != "a designar")
                {
                    segunda.Append($" Vehículo(s) habilitado(s) al momento de la firma: {m.VehiculosLabel}.");
                }

                Clausula(col, "SEGUNDA", segunda.ToString());

                Clausula(col, "TERCERA",
                    $"El plazo de la locación es de {m.PlazoMeses} ({NumeroEnLetras(m.PlazoMeses)}) mes(es), contados desde el {Fmt(m.FechaInicio)} hasta el {Fmt(m.FechaFin)}, renovable por acuerdo expreso de las partes. La operatoria del abono en el sistema Parkking se mantiene vigente hasta su baja, independientemente del plazo contractual aquí estipulado.");

                var moraTexto = m.AplicaRecargo && m.PorcentajeRecargo > 0
                    ? $"El incumplimiento del pago en término generará un recargo del {m.PorcentajeRecargo:0.##}% sobre el monto adeudado, conforme la configuración del estacionamiento."
                    : "El incumplimiento del pago en término podrá generar los recargos o intereses que EL LOCADOR determine conforme a la normativa aplicable y a las políticas del estacionamiento.";

                Clausula(col, "CUARTA",
                    $"EL LOCATARIO abonará la suma de {m.PrecioTexto} por período ({m.PeriodicidadLabel}), por adelantado, dentro de los primeros {m.DiaVencimiento} ({NumeroEnLetras(m.DiaVencimiento)}) días de cada período de cobro. El pago podrá realizarse en efectivo, transferencia u otros medios habilitados por EL LOCADOR. {moraTexto}");

                var quinta = new StringBuilder();
                quinta.Append("El presente contrato no incluye servicio de vigilancia, custodia ni guarda del vehículo. EL LOCADOR no responde por daños, hurtos o robos del vehículo o de objetos dejados en su interior, salvo dolo o culpa grave debidamente acreditada. EL LOCATARIO responde por los daños que cause al inmueble o a terceros con motivo de las maniobras de ingreso, egreso o estadía.");
                if (m.SeguroObligatorio)
                {
                    quinta.Append(" Es condición esencial de este contrato que el/los vehículo(s) cuenten con seguro vigente durante toda la locación.");
                }

                Clausula(col, "QUINTA", quinta.ToString());

                Clausula(col, "SEXTA",
                    "EL LOCATARIO recibirá, cuando corresponda, llave y/o control de acceso de uso personal e intransferible. En caso de pérdida o deterioro, abonará el costo de reposición. El acceso al estacionamiento se permite las veinticuatro (24) horas, todos los días del año, salvo fuerza mayor o disposiciones de autoridad competente.");

                Clausula(col, "SÉPTIMA",
                    "EL LOCADOR se ocupa de las reparaciones estructurales del inmueble. EL LOCATARIO debe mantener el espacio en buen estado de conservación, usar las instalaciones conforme a su destino y comunicar de inmediato cualquier desperfecto.");

                Clausula(col, "OCTAVA",
                    "EL LOCATARIO podrá rescindir el contrato con un preaviso mínimo de quince (15) días. EL LOCADOR podrá rescindir por: a) falta de pago; b) uso indebido del espacio; c) incumplimiento de las obligaciones del presente; d) necesidad de destinar el espacio a otro uso, mediando preaviso escrito a EL LOCATARIO.");

                Clausula(col, "NOVENA",
                    "Para cualquier controversia derivada de este contrato, las partes se someten a los tribunales ordinarios con competencia en el domicilio del inmueble locado, renunciando a cualquier otro fuero que pudiera corresponderles.");

                Clausula(col, "DÉCIMA",
                    "Las partes constituyen domicilio especial en los indicados en el exordio, donde serán válidas todas las notificaciones judiciales o extrajudiciales.");

                col.Item().PaddingTop(4).Text(text =>
                {
                    text.Justify();
                    text.Span(
                        $"En prueba de conformidad, se firman dos (2) ejemplares de un mismo tenor y a un solo efecto, en la ciudad correspondiente al domicilio del inmueble, a los {m.FechaFirma.Day} ({NumeroEnLetras(m.FechaFirma.Day)}) días del mes de {MesNombre(m.FechaFirma.Month)} de {m.FechaFirma.Year}.");
                });

                col.Item().PaddingTop(36).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("_______________________________");
                        c.Item().AlignCenter().PaddingTop(6).Text("EL LOCADOR").Bold().FontSize(10);
                        c.Item().AlignCenter().PaddingTop(2).Text(m.LocadorNombre).FontSize(9);
                        c.Item().AlignCenter().Text($"DNI/CUIT {m.LocadorDocumento}").FontSize(9);
                    });
                    row.ConstantItem(28);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text("_______________________________");
                        c.Item().AlignCenter().PaddingTop(6).Text("EL LOCATARIO").Bold().FontSize(10);
                        c.Item().AlignCenter().PaddingTop(2).Text(m.LocatarioNombre).FontSize(9);
                        c.Item().AlignCenter().Text($"DNI {m.LocatarioDocumento}").FontSize(9);
                    });
                });
            });

            page.Footer().AlignCenter().DefaultTextStyle(x => x
                .FontFamily("Times New Roman")
                .FontSize(9)
                .FontColor(Colors.Black)).Text(t =>
            {
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
                t.Span($"  —  Abono N.º {m.AbonoId}");
            });
        });
    }

    /// <summary>Estilo clásico AR: rótulo de cláusula en negrita seguido del cuerpo justificado.</summary>
    private static void Clausula(ColumnDescriptor col, string numero, string cuerpo)
    {
        col.Item().Text(text =>
        {
            text.Justify();
            text.Span($"{numero}.- ").Bold();
            text.Span(cuerpo);
        });
    }

    private static string PrimeroNoVacio(string? a, string? b)
    {
        if (!string.IsNullOrWhiteSpace(a)) return a.Trim();
        if (!string.IsNullOrWhiteSpace(b)) return b.Trim();
        return "—";
    }

    private static string Fmt(DateOnly d) => d.ToString("dd/MM/yyyy", new CultureInfo("es-AR"));

    private static string MesNombre(int mes) =>
        new CultureInfo("es-AR").DateTimeFormat.GetMonthName(mes);

    private static string LabelPeriodicidad(PeriodicidadCobro p) => p switch
    {
        PeriodicidadCobro.Mensual => "mensual",
        PeriodicidadCobro.Quincenal => "quincenal",
        PeriodicidadCobro.Bimestral => "bimestral",
        PeriodicidadCobro.Trimestral => "trimestral",
        PeriodicidadCobro.Semestral => "semestral",
        PeriodicidadCobro.Anual => "anual",
        _ => "periódico"
    };

    private static string NumeroEnLetras(int n) => n switch
    {
        1 => "uno",
        2 => "dos",
        3 => "tres",
        4 => "cuatro",
        5 => "cinco",
        6 => "seis",
        7 => "siete",
        8 => "ocho",
        9 => "nueve",
        10 => "diez",
        11 => "once",
        12 => "doce",
        13 => "trece",
        14 => "catorce",
        15 => "quince",
        16 => "dieciséis",
        17 => "diecisiete",
        18 => "dieciocho",
        19 => "diecinueve",
        20 => "veinte",
        21 => "veintiuno",
        22 => "veintidós",
        23 => "veintitrés",
        24 => "veinticuatro",
        25 => "veinticinco",
        26 => "veintiséis",
        27 => "veintisiete",
        28 => "veintiocho",
        29 => "veintinueve",
        30 => "treinta",
        31 => "treinta y uno",
        _ => n.ToString()
    };

    private sealed class ContratoPdfModel
    {
        public string EstacionamientoNombre { get; set; } = "";
        public string InmuebleDireccion { get; set; } = "";
        public string LocadorNombre { get; set; } = "";
        public string LocadorDocumento { get; set; } = "";
        public string LocadorDomicilio { get; set; } = "";
        public string LocatarioNombre { get; set; } = "";
        public string LocatarioDocumento { get; set; } = "";
        public string LocatarioDomicilio { get; set; } = "";
        public string PlazasLabel { get; set; } = "";
        public int CantidadPlazas { get; set; }
        public string VehiculosLabel { get; set; } = "";
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public int PlazoMeses { get; set; }
        public string PrecioTexto { get; set; } = "";
        public string PeriodicidadLabel { get; set; } = "";
        public int DiaVencimiento { get; set; }
        public bool AplicaRecargo { get; set; }
        public decimal PorcentajeRecargo { get; set; }
        public bool SeguroObligatorio { get; set; }
        public DateOnly FechaFirma { get; set; }
        public int AbonoId { get; set; }
    }
}
