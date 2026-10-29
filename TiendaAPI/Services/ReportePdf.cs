using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaAPI.Helpers;

namespace TiendaAPI.Services
{
    // Arma un PDF con formato de tabla usando QuestPDF.
    // Todos los reportes (stock, ingresos, ventas) usan este mismo formato:
    // título + subtítulo, tabla con encabezado, líneas de resumen y número de página.
    public static class ReportePdf
    {
        public static readonly CultureInfo Cultura = new("es-AR");

        public record Columna(string Titulo, float Ancho = 1, bool AlinearDerecha = false);

        public static string Moneda(decimal valor) => valor.ToString("C2", Cultura);

        public static string Fecha(DateTime fechaUtc) => FechaHelper.ALocal(fechaUtc).ToString("dd/MM/yyyy HH:mm", Cultura);

        public static byte[] Generar(string titulo, string subtitulo, IList<Columna> columnas,
            IEnumerable<string[]> filas, IEnumerable<string> resumen)
        {
            var generado = FechaHelper.ALocal(DateTime.UtcNow).ToString("dd/MM/yyyy HH:mm", Cultura);

            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(30);
                    pagina.DefaultTextStyle(t => t.FontSize(9));

                    pagina.Header().PaddingBottom(10).Column(col =>
                    {
                        col.Item().Text("Tienda - Sistema de gestión").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(titulo).FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text(subtitulo).FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"Generado: {generado}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    pagina.Content().Column(col =>
                    {
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(def =>
                            {
                                foreach (var c in columnas) def.RelativeColumn(c.Ancho);
                            });

                            tabla.Header(encabezado =>
                            {
                                foreach (var c in columnas)
                                {
                                    var celda = encabezado.Cell().Background(Colors.Blue.Darken3).Padding(4);
                                    (c.AlinearDerecha ? celda.AlignRight() : celda).Text(c.Titulo).Bold().FontColor(Colors.White);
                                }
                            });

                            var hayFilas = false;
                            foreach (var fila in filas)
                            {
                                hayFilas = true;
                                for (var i = 0; i < columnas.Count; i++)
                                {
                                    var celda = tabla.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                                    (columnas[i].AlinearDerecha ? celda.AlignRight() : celda).Text(fila[i]);
                                }
                            }

                            if (!hayFilas)
                                tabla.Cell().ColumnSpan((uint)columnas.Count).Padding(10).AlignCenter()
                                    .Text("No hay datos para el período o filtro elegido").Italic();
                        });

                        col.Item().PaddingTop(10).AlignRight().Column(res =>
                        {
                            foreach (var linea in resumen)
                                res.Item().Text(linea).Bold();
                        });
                    });

                    pagina.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}
