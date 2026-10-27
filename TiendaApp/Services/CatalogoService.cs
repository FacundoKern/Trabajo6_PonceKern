using System.Net.Http.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

public class CatalogoService
{
    private readonly HttpClient _http;
    private List<Categoria>? _categorias;

    public CatalogoService(HttpClient http)
    {
        _http = http;
    }

    // Solo productos activos: los dados de baja no se muestran en el catálogo
    public async Task<PaginaResultado<Producto>> BuscarProductosAsync(string? buscar, int? categoriaId, int pagina, int tamanioPagina)
    {
        var url = $"api/productos?soloActivos=true&pagina={pagina}&tamanioPagina={tamanioPagina}";
        if (!string.IsNullOrWhiteSpace(buscar))
            url += "&buscar=" + Uri.EscapeDataString(buscar.Trim());
        if (categoriaId.HasValue)
            url += "&categoriaId=" + categoriaId.Value;

        return await _http.GetFromJsonAsync<PaginaResultado<Producto>>(url)
            ?? new PaginaResultado<Producto>();
    }

    public async Task<Producto?> ObtenerProductoAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/productos/{id}");
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Producto>();
    }

    // Las categorías cambian poco: se piden una vez y quedan en memoria
    public async Task<List<Categoria>> ObtenerCategoriasAsync()
    {
        if (_categorias is null)
        {
            var resultado = await _http.GetFromJsonAsync<PaginaResultado<Categoria>>("api/categorias?tamanioPagina=50");
            _categorias = resultado?.Items ?? new();
        }
        return _categorias;
    }
}
