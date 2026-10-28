using System.Net.Http.Json;
using System.Text.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

public class VentasService
{
    private readonly HttpClient _http;
    private int? _consumidorFinalId;

    public VentasService(HttpClient http)
    {
        _http = http;
    }

    // Las compras desde la app se registran a nombre del cliente "Consumidor Final" (lo crea la API)
    public async Task<int?> ObtenerConsumidorFinalIdAsync()
    {
        if (_consumidorFinalId is null)
        {
            var resultado = await _http.GetFromJsonAsync<PaginaResultado<Cliente>>("api/clientes?buscar=Consumidor&tamanioPagina=50");
            _consumidorFinalId = resultado?.Items
                .FirstOrDefault(c => c.Nombre == "Consumidor" && c.Apellido == "Final")?.Id;
        }
        return _consumidorFinalId;
    }

    // Devuelve la venta registrada, o el mensaje de error para mostrar (stock, pago rechazado, etc.)
    public async Task<(Venta? Venta, string? Error)> RegistrarAsync(VentaRequest venta)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.PostAsJsonAsync("api/salidas", venta);
        }
        catch (Exception)
        {
            return (null, "No se pudo conectar con el servidor. Revisá la conexión a internet.");
        }

        if (respuesta.IsSuccessStatusCode)
            return (await respuesta.Content.ReadFromJsonAsync<Venta>(), null);

        return (null, await LeerErrorAsync(respuesta));
    }

    public async Task<Venta?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/salidas/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Venta>();
    }

    // La API responde los errores como { mensaje, errores: [...] }
    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            var raiz = json.RootElement;

            if (raiz.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Array && errores.GetArrayLength() > 0)
                return string.Join("\n", errores.EnumerateArray().Select(e => e.GetString()));

            if (raiz.TryGetProperty("mensaje", out var mensaje))
                return mensaje.GetString() ?? "Error desconocido";
        }
        catch (JsonException)
        {
        }
        return $"El servidor respondió con un error (HTTP {(int)respuesta.StatusCode}).";
    }
}
