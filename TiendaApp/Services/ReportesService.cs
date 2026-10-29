namespace TiendaApp.Services;

// Baja los reportes PDF que arma la API (QuestPDF) y los guarda en el celular para abrirlos o mandarlos
public class ReportesService
{
    private readonly HttpClient _http;

    public ReportesService(HttpClient http)
    {
        _http = http;
    }

    // Cualquier usuario logueado
    public Task<(string? Ruta, string? Error)> DescargarStockAsync(int? categoriaId, int stockMinimo)
    {
        var url = $"api/reportes/stock?stockMinimo={stockMinimo}";
        if (categoriaId.HasValue)
            url += "&categoriaId=" + categoriaId.Value;
        return DescargarAsync(url, "reporte_stock.pdf");
    }

    // Solo Admin. Las fechas van como día; la API las toma en hora argentina
    public Task<(string? Ruta, string? Error)> DescargarVentasAsync(DateTime desde, DateTime hasta) =>
        DescargarAsync($"api/reportes/ventas{Rango(desde, hasta)}", "reporte_ventas.pdf");

    public Task<(string? Ruta, string? Error)> DescargarIngresosAsync(DateTime desde, DateTime hasta) =>
        DescargarAsync($"api/reportes/ingresos{Rango(desde, hasta)}", "reporte_ingresos.pdf");

    // Abre el PDF con el visor que tenga el celular
    public Task AbrirAsync(string ruta) =>
        Launcher.Default.OpenAsync(new OpenFileRequest("Reporte", new ReadOnlyFile(ruta, "application/pdf")));

    // Abre la app de correo con el PDF adjunto. Si el celular no tiene ninguna configurada,
    // ofrece compartirlo (Gmail, WhatsApp, Drive, etc.)
    public async Task EnviarPorEmailAsync(string ruta, string? destinatario)
    {
        var nombre = Path.GetFileName(ruta);

        if (Email.Default.IsComposeSupported)
        {
            var mensaje = new EmailMessage
            {
                Subject = $"Reporte {nombre}",
                Body = "Te envío el reporte generado desde la app de la tienda.",
                To = string.IsNullOrWhiteSpace(destinatario) ? new() : new() { destinatario.Trim() }
            };
            mensaje.Attachments!.Add(new EmailAttachment(ruta, "application/pdf"));
            await Email.Default.ComposeAsync(mensaje);
        }
        else
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Enviar {nombre}",
                File = new ShareFile(ruta, "application/pdf")
            });
        }
    }

    private async Task<(string? Ruta, string? Error)> DescargarAsync(string url, string nombrePorDefecto)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.GetAsync(url);
        }
        catch (Exception)
        {
            return (null, ApiErrores.SinConexion);
        }

        if (!respuesta.IsSuccessStatusCode)
            return (null, await ApiErrores.LeerAsync(respuesta));

        // La API manda el nombre con fecha y hora (reporte_ventas_20261029_1530.pdf)
        var disposicion = respuesta.Content.Headers.ContentDisposition;
        var nombre = Path.GetFileName((disposicion?.FileNameStar ?? disposicion?.FileName ?? nombrePorDefecto).Trim('"'));

        // Se guardan en la caché de la app: Android la puede limpiar, y antes de cada descarga se borran los viejos
        var carpeta = Path.Combine(FileSystem.Current.CacheDirectory, "reportes");
        if (Directory.Exists(carpeta))
            Directory.Delete(carpeta, recursive: true);
        Directory.CreateDirectory(carpeta);

        var ruta = Path.Combine(carpeta, nombre);
        await using (var archivo = File.Create(ruta))
            await respuesta.Content.CopyToAsync(archivo);

        return (ruta, null);
    }

    private static string Rango(DateTime desde, DateTime hasta) =>
        $"?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
}
