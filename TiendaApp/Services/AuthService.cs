using System.Net;
using System.Net.Http.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly SesionService _sesion;

    public AuthService(HttpClient http, SesionService sesion)
    {
        _http = http;
        _sesion = sesion;
    }

    // Devuelve null si el login salió bien, o el mensaje de error para mostrar
    public async Task<string?> LoginAsync(LoginRequest datos)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.PostAsJsonAsync("api/auth/login", datos);
        }
        catch (Exception)
        {
            return "No se pudo conectar con el servidor. Revisá la conexión a internet.";
        }

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return "Usuario o contraseña incorrectos.";
        if (!respuesta.IsSuccessStatusCode)
            return $"El servidor respondió con un error (HTTP {(int)respuesta.StatusCode}).";

        var sesion = await respuesta.Content.ReadFromJsonAsync<Sesion>();
        if (sesion is null || string.IsNullOrEmpty(sesion.Token))
            return "La respuesta del servidor no es válida.";

        await _sesion.IniciarSesionAsync(sesion);
        return null;
    }

    public Task LogoutAsync() => _sesion.CerrarSesionAsync();
}
