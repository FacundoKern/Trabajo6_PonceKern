using System.Net.Http.Json;
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
    public Task<(Venta? Venta, string? Error)> RegistrarAsync(VentaRequest venta) =>
        ApiErrores.EnviarAsync<Venta>(() => _http.PostAsJsonAsync("api/salidas", venta));

    // Historial: la API las devuelve de la más nueva a la más vieja
    public async Task<PaginaResultado<Venta>> ListarAsync(int pagina, int tamanioPagina) =>
        await _http.GetFromJsonAsync<PaginaResultado<Venta>>($"api/salidas?pagina={pagina}&tamanioPagina={tamanioPagina}")
            ?? new PaginaResultado<Venta>();

    public async Task<Venta?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/salidas/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Venta>();
    }
}
