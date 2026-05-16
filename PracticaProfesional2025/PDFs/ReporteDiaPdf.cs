using PracticaProfesional2025.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PracticaProfesional2025.PDFs
{
    public class ReporteDiaPdf : IDocument
    {
        private readonly List<DetalleVenta> _detalles;
        private readonly DateTime _fecha;

        public ReporteDiaPdf(List<DetalleVenta> detalles, DateTime fecha)
        {
            _detalles = detalles;
            _fecha = fecha;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            decimal totalDia = _detalles.Sum(x => x.Subtotal ?? 0);

            var productoMasVendido = _detalles
                .GroupBy(x => x.Producto?.Nombre)
                .Select(g => new
                {
                    Producto = g.Key,
                    Cantidad = g.Sum(x => x.Cantidad),
                    Total = g.Sum(x => x.Subtotal ?? 0)
                })
                .OrderByDescending(x => x.Cantidad)
                .FirstOrDefault();

            container.Page(page =>
            {
                page.Margin(35);

                // =========================
                // HEADER
                // =========================
                page.Header().Column(header =>
                {
                    header.Item().Text("LA PERLA")
                        .Bold()
                        .FontSize(24);

                    header.Item().Text("Sistema de Ventas")
                        .FontSize(12);

                    header.Item().PaddingTop(10);

                    header.Item().Text($"Reporte diario - {_fecha:dd/MM/yyyy}")
                        .SemiBold()
                        .FontSize(18);

                    header.Item().LineHorizontal(1);
                });

                // =========================
                // CONTENT
                // =========================
                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(18);

                    // =====================
                    // RESUMEN
                    // =====================
                    column.Item().Border(1).Padding(15).Column(resumen =>
                    {
                        resumen.Spacing(8);

                        resumen.Item().Text("Resumen del día")
                            .Bold()
                            .FontSize(16);

                        resumen.Item().Text($"Cantidad de registros: {_detalles.Count}");

                        resumen.Item().Text($"Total generado: ${totalDia:0.##}");

                        if (productoMasVendido != null)
                        {
                            resumen.Item().Text(
                                $"Producto más vendido: {productoMasVendido.Producto}"
                            );

                            resumen.Item().Text(
                                $"Cantidad vendida: {productoMasVendido.Cantidad:0.##}"
                            );

                            resumen.Item().Text(
                                $"Total producido: ${productoMasVendido.Total:0.##}"
                            );
                        }
                    });

                    // =====================
                    // TABLA
                    // =====================
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // HEADER TABLA
                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).Padding(5)
                                .Text("Producto").Bold();

                            header.Cell().BorderBottom(1).Padding(5)
                                .AlignCenter()
                                .Text("Cantidad").Bold();

                            header.Cell().BorderBottom(1).Padding(5)
                                .AlignCenter()
                                .Text("Precio").Bold();

                            header.Cell().BorderBottom(1).Padding(5)
                                .AlignRight()
                                .Text("Subtotal").Bold();
                        });

                        // FILAS
                        foreach (var item in _detalles)
                        {
                            table.Cell().PaddingVertical(6)
                                .Text(item.Producto?.Nombre ?? "");

                            table.Cell().PaddingVertical(6)
                                .AlignCenter()
                                .Text(item.Cantidad.ToString("0.##"));

                            table.Cell().PaddingVertical(6)
                                .AlignCenter()
                                .Text($"${item.Producto?.Precio:0.##}");

                            table.Cell().PaddingVertical(6)
                                .AlignRight()
                                .Text($"${item.Subtotal:0.##}");
                        }
                    });

                    // =====================
                    // TOTAL FINAL
                    // =====================
                    column.Item()
                        .AlignRight()
                        .Border(1)
                        .Padding(10)
                        .Text($"TOTAL DEL DÍA: ${totalDia:0.##}")
                        .Bold()
                        .FontSize(18);
                });

                // =========================
                // FOOTER
                // =========================
                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("La Perla - Reporte generado automáticamente | Página ");
                        x.CurrentPageNumber();
                    });
            });
        }
    }
}