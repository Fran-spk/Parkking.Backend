using System.Globalization;
using ClosedXML.Excel;
using Parkking.Infrastructure.Tenant;
using Parkking.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Parkking.Services;

public class ReportesService
{
    private readonly DashboardService _dashboard;
    private readonly PagoRepository _pagos;
    private readonly IEstacionamientoContext _estacionamiento;

    static ReportesService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReportesService(
        DashboardService dashboard,
        PagoRepository pagos,
        IEstacionamientoContext estacionamiento)
    {
        _dashboard = dashboard;
        _pagos = pagos;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    private string NombreEstacionamiento() =>
        _pagos.GetEstacionamiento(TenantId)?.Nombre ?? $"Estacionamiento {TenantId}";

    public (byte[] Bytes, string FileName) GenerarDashboardPdf()
    {
        var data = _dashboard.GetSummary();
        var nombre = NombreEstacionamiento();
        var generado = DateTime.Now;
        var cultura = new CultureInfo("es-AR");

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Text("Parkking · Resumen operativo").SemiBold().FontSize(16).FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"{nombre}").FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Generado: {generado:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Text("Indicadores").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });
                        KpiCell(t, "Ocupación", $"{data.PorcentajeOcupacion:0.#}%");
                        KpiCell(t, "Cocheras", $"{data.CocherasOcupadas} / {data.CocherasTotales}");
                        KpiCell(t, "Abonos activos", data.CantidadAbonosActivos.ToString());
                        KpiCell(t, "Recaudado mes", data.DetalleIngresos.TotalRecaudadoMensualReal.ToString("C0", cultura));
                        KpiCell(t, "Proyección", data.DetalleIngresos.TotalEstimadoProyeccion.ToString("C0", cultura));
                        KpiCell(t, "Dif. %", $"{data.DetalleIngresos.DiferenciaPorcentaje:0.#}%");
                    });

                    col.Item().Text("Deudores").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    if (data.AlertasDeudores.Count == 0)
                    {
                        col.Item().Text("Sin deudores.").Italic().FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.2f);
                                c.RelativeColumn(1.2f);
                                c.RelativeColumn(0.8f);
                                c.RelativeColumn(0.8f);
                                c.RelativeColumn(1.2f);
                            });
                            HeaderRow(t, "Cliente", "Cochera", "Días", "Períodos", "Saldo");
                            foreach (var d in data.AlertasDeudores)
                            {
                                t.Cell().Element(CellBody).Text(d.ClienteNombre);
                                t.Cell().Element(CellBody).Text(d.CocheraNumero);
                                t.Cell().Element(CellBody).AlignRight().Text(d.DiasAtraso.ToString());
                                t.Cell().Element(CellBody).AlignRight().Text(d.PeriodosAdeudados.ToString());
                                t.Cell().Element(CellBody).AlignRight().Text(d.SaldoTotal.ToString("C0", cultura));
                            }
                        });
                    }

                    col.Item().Text("Distribución de vehículos").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    if (data.DistribucionVehiculos.Count == 0)
                    {
                        col.Item().Text("Sin vehículos en abonos activos.").Italic().FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            HeaderRow(t, "Tipo", "Cantidad", "%");
                            foreach (var v in data.DistribucionVehiculos)
                            {
                                t.Cell().Element(CellBody).Text(v.TipoVehiculo);
                                t.Cell().Element(CellBody).AlignRight().Text(v.Cantidad.ToString());
                                t.Cell().Element(CellBody).AlignRight().Text($"{v.Porcentaje:0.#}%");
                            }
                        });
                    }

                    col.Item().Text("Ingresos últimos 6 meses").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });
                        HeaderRow(t, "Mes", "Monto");
                        foreach (var h in data.GraficoIngresos)
                        {
                            t.Cell().Element(CellBody).Text(h.Mes);
                            t.Cell().Element(CellBody).AlignRight().Text(h.Monto.ToString("C0", cultura));
                        }
                    });

                    col.Item().Text("Estado de cocheras").SemiBold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(0.7f);
                            c.RelativeColumn(1.1f);
                            c.RelativeColumn(0.6f);
                            c.RelativeColumn(2.2f);
                            c.RelativeColumn(1.4f);
                        });
                        HeaderRow(t, "Nº", "Estado", "Abonos", "Clientes", "Patentes");
                        foreach (var c in data.EstadoCocheras)
                        {
                            t.Cell().Element(CellBody).Text(c.Numero);
                            t.Cell().Element(CellBody).Text(c.Estado);
                            t.Cell().Element(CellBody).AlignRight().Text(c.AbonosActivos.ToString());
                            t.Cell().Element(CellBody).Text(c.ClienteNombre ?? "—");
                            t.Cell().Element(CellBody).Text(
                                string.IsNullOrWhiteSpace(c.Patente) ? "—" : c.Patente);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" / ");
                    txt.TotalPages();
                });
            });
        }).GeneratePdf();

        return (bytes, $"dashboard-{generado:yyyyMMdd-HHmm}.pdf");
    }

    public (byte[] Bytes, string FileName) GenerarPagosExcel(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var d = desde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var h = hasta ?? hoy;
        if (h < d)
            throw new Exception("La fecha hasta no puede ser anterior a desde.");

        var pagos = _pagos.GetByFechaCobro(TenantId, d, h);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Pagos");

        ws.Cell(1, 1).Value = "Parkking · Reporte de pagos";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 12).Merge();

        ws.Cell(2, 1).Value = NombreEstacionamiento();
        ws.Cell(3, 1).Value = $"Período cobro: {d:dd/MM/yyyy} – {h:dd/MM/yyyy}";
        ws.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var headers = new[]
        {
            "Fecha cobro", "Cliente", "Cocheras", "Períodos", "Patentes",
            "Monto", "Recargo", "Total", "Método", "Observación", "Recibo", "AbonoId"
        };

        const int headerRow = 6;
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a8a");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = headerRow + 1;
        foreach (var p in pagos)
        {
            var cocheras = string.Join(", ",
                p.Abono?.Plazas?.Where(pl => pl.Activo).Select(pl => pl.Cochera?.Numero).Where(n => n != null)
                ?? Enumerable.Empty<string>());
            var periodos = string.Join(", ",
                p.Detalles?
                    .OrderBy(d => d.Cuota?.PeriodoInicio)
                    .Select(d => d.Cuota?.PeriodoInicio.ToString("dd/MM/yyyy") ?? "")
                    .Where(s => s.Length > 0)
                ?? Enumerable.Empty<string>());
            var patentes = string.Join(", ",
                p.Abono?.AbonoVehiculos?.Select(av => av.Vehiculo?.Patente).Where(x => !string.IsNullOrWhiteSpace(x))
                ?? Enumerable.Empty<string>());
            var recargo = p.Recargo ?? 0;
            var total = p.MontoTotal + recargo;

            ws.Cell(row, 1).Value = p.FechaHora;
            ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            ws.Cell(row, 2).Value = p.Abono?.Cliente?.Nombre ?? "";
            ws.Cell(row, 3).Value = cocheras;
            ws.Cell(row, 4).Value = periodos;
            ws.Cell(row, 5).Value = patentes;
            ws.Cell(row, 6).Value = p.MontoTotal;
            ws.Cell(row, 6).Style.NumberFormat.Format = "\"$\"#,##0.00";
            ws.Cell(row, 7).Value = recargo;
            ws.Cell(row, 7).Style.NumberFormat.Format = "\"$\"#,##0.00";
            ws.Cell(row, 8).Value = total;
            ws.Cell(row, 8).Style.NumberFormat.Format = "\"$\"#,##0.00";
            ws.Cell(row, 9).Value = p.MetodoDePago?.Nombre ?? "";
            ws.Cell(row, 10).Value = p.Observacion ?? "";
            ws.Cell(row, 11).Value = p.Recibo?.NumeroFormateado ?? "";
            ws.Cell(row, 12).Value = p.AbonoId;
            row++;
        }

        if (pagos.Count > 0)
        {
            ws.Cell(row, 5).Value = "TOTAL";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 6).FormulaA1 = $"SUM(F{headerRow + 1}:F{row - 1})";
            ws.Cell(row, 7).FormulaA1 = $"SUM(G{headerRow + 1}:G{row - 1})";
            ws.Cell(row, 8).FormulaA1 = $"SUM(H{headerRow + 1}:H{row - 1})";
            ws.Range(row, 6, row, 8).Style.Font.Bold = true;
            ws.Range(row, 6, row, 8).Style.NumberFormat.Format = "\"$\"#,##0.00";
        }
        else
        {
            ws.Cell(row, 1).Value = "Sin pagos en el período.";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return (ms.ToArray(), $"pagos-{d:yyyyMMdd}-{h:yyyyMMdd}.xlsx");
    }

    private static void KpiCell(TableDescriptor t, string label, string value)
    {
        t.Cell().Element(e => e
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten4)
            .Padding(8)
            .Column(c =>
            {
                c.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
                c.Item().PaddingTop(2).Text(value).SemiBold().FontSize(11);
            }));
    }

    private static void HeaderRow(TableDescriptor t, params string[] titles)
    {
        foreach (var title in titles)
            t.Cell().Element(CellHeader).Text(title);
    }

    private static IContainer CellHeader(IContainer c) =>
        c.Background(Colors.Blue.Darken3)
            .Padding(5)
            .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White).FontSize(8));

    private static IContainer CellBody(IContainer c) =>
        c.BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(5);
}
