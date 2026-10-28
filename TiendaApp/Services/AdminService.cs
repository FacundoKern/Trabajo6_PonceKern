using System.Net;
using System.Net.Http.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

// Usuarios, roles, auditoría y log de errores. Todos estos endpoints son solo para Admin
public class AdminService
{
    private readonly HttpClient _http;

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    // ---------- Usuarios ----------

    public async Task<PaginaResultado<Usuario>> BuscarUsuariosAsync(string? buscar, int pagina, int tamanioPagina) =>
        await _http.GetFromJsonAsync<PaginaResultado<Usuario>>(
            $"api/usuarios?pagina={pagina}&tamanioPagina={tamanioPagina}{Buscar(buscar)}")
            ?? new PaginaResultado<Usuario>();

    public async Task<Usuario?> ObtenerUsuarioAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/usuarios/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<Usuario>();
    }

    // El alta sigue estando en api/auth/registro
    public Task<string?> CrearUsuarioAsync(NuevoUsuario usuario) =>
        ApiErrores.EnviarAsync(() => _http.PostAsJsonAsync("api/auth/registro", usuario));

    public Task<string?> ActualizarUsuarioAsync(int id, UsuarioEditar usuario) =>
        ApiErrores.EnviarAsync(() => _http.PutAsJsonAsync($"api/usuarios/{id}", usuario));

    public Task<string?> CambiarPasswordAsync(int id, CambiarPassword datos) =>
        ApiErrores.EnviarAsync(() => _http.PutAsJsonAsync($"api/usuarios/{id}/password", datos));

    // ---------- Roles ----------

    public async Task<List<Rol>> ObtenerRolesAsync() =>
        await _http.GetFromJsonAsync<List<Rol>>("api/roles") ?? new();

    public Task<string?> CrearRolAsync(RolRequest rol) =>
        ApiErrores.EnviarAsync(() => _http.PostAsJsonAsync("api/roles", rol));

    public Task<string?> ActualizarRolAsync(int id, RolRequest rol) =>
        ApiErrores.EnviarAsync(() => _http.PutAsJsonAsync($"api/roles/{id}", rol));

    public Task<string?> EliminarRolAsync(int id) =>
        ApiErrores.EnviarAsync(() => _http.DeleteAsync($"api/roles/{id}"));

    // ---------- Auditoría y errores ----------

    // Las fechas se mandan como día (yyyy-MM-dd); la API las toma en hora argentina
    public async Task<PaginaResultado<RegistroAuditoria>> BuscarAuditoriaAsync(
        string? buscar, DateTime? desde, DateTime? hasta, int pagina, int tamanioPagina)
    {
        var url = $"api/auditoria?pagina={pagina}&tamanioPagina={tamanioPagina}{Buscar(buscar)}";
        if (desde.HasValue)
            url += "&desde=" + desde.Value.ToString("yyyy-MM-dd");
        if (hasta.HasValue)
            url += "&hasta=" + hasta.Value.ToString("yyyy-MM-dd");

        return await _http.GetFromJsonAsync<PaginaResultado<RegistroAuditoria>>(url)
            ?? new PaginaResultado<RegistroAuditoria>();
    }

    public async Task<PaginaResultado<LogError>> BuscarErroresAsync(string? buscar, int pagina, int tamanioPagina) =>
        await _http.GetFromJsonAsync<PaginaResultado<LogError>>(
            $"api/auditoria/errores?pagina={pagina}&tamanioPagina={tamanioPagina}{Buscar(buscar)}")
            ?? new PaginaResultado<LogError>();

    public async Task<LogError?> ObtenerErrorAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/auditoria/errores/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<LogError>();
    }

    // La API tira una excepción a propósito para mostrar que el middleware la guarda en LogErrores.
    // Lo esperado es un 500; cualquier otra respuesta se informa como error
    public async Task<string?> ProbarErrorAsync()
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.PostAsync("api/auditoria/probar-error", null);
        }
        catch (Exception)
        {
            return ApiErrores.SinConexion;
        }

        return respuesta.StatusCode == HttpStatusCode.InternalServerError
            ? null
            : await ApiErrores.LeerAsync(respuesta);
    }

    private static string Buscar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? string.Empty : "&buscar=" + Uri.EscapeDataString(texto.Trim());
}
