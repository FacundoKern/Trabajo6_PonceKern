using System.Net.Http.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

// Ingresos de mercadería: compras a proveedores que suman stock
public class IngresosService
{
    private readonly HttpClient _http;

    public IngresosService(HttpClient http)
    {
        _http = http;
    }

    // La API devuelve también los dados de baja; para cargar un ingreso solo sirven los activos
    public async Task<List<Proveedor>> ObtenerProveedoresActivosAsync()
    {
        var resultado = await _http.GetFromJsonAsync<PaginaResultado<Proveedor>>("api/proveedores?tamanioPagina=50");
        return resultado?.Items.Where(p => p.Activo).ToList() ?? new();
    }

    public async Task<PaginaResultado<Ingreso>> ListarAsync(int pagina, int tamanioPagina) =>
        await _http.GetFromJsonAsync<PaginaResultado<Ingreso>>($"api/ingresos?pagina={pagina}&tamanioPagina={tamanioPagina}")
            ?? new PaginaResultado<Ingreso>();

    public async Task<Ingreso?> ObtenerAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/ingresos/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Ingreso>();
    }

    // Solo Admin. Devuelve el ingreso registrado, o el mensaje de error para mostrar
    public Task<(Ingreso? Ingreso, string? Error)> RegistrarAsync(IngresoRequest ingreso) =>
        ApiErrores.EnviarAsync<Ingreso>(() => _http.PostAsJsonAsync("api/ingresos", ingreso));
}
