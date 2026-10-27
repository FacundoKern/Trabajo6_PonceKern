using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TiendaApp.Models;

namespace TiendaApp.Services;

// Guarda la sesión (token JWT y rol) en el almacenamiento seguro del teléfono
// y le informa a Blazor quién está logueado, para que funcionen [Authorize] y <AuthorizeView>.
public class SesionService : AuthenticationStateProvider
{
    private const string ClaveStorage = "sesion";

    private Sesion? _actual;
    private Task? _carga;

    public Sesion? Actual => _actual;
    public bool EsAdmin => _actual?.Rol == Roles.Admin;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await CargarAsync();
        return CrearEstado(_actual);
    }

    // El token se lee una sola vez del almacenamiento; después queda en memoria
    public async Task<string?> ObtenerTokenAsync()
    {
        await CargarAsync();
        return _actual?.Token;
    }

    public async Task IniciarSesionAsync(Sesion sesion)
    {
        _actual = sesion;
        try
        {
            await SecureStorage.Default.SetAsync(ClaveStorage, JsonSerializer.Serialize(sesion));
        }
        catch
        {
            // Si el dispositivo no tiene almacenamiento seguro, la sesión dura mientras la app esté abierta
        }
        NotifyAuthenticationStateChanged(Task.FromResult(CrearEstado(sesion)));
    }

    public Task CerrarSesionAsync()
    {
        if (_actual is null) return Task.CompletedTask;

        _actual = null;
        try
        {
            SecureStorage.Default.Remove(ClaveStorage);
        }
        catch
        {
        }
        NotifyAuthenticationStateChanged(Task.FromResult(CrearEstado(null)));
        return Task.CompletedTask;
    }

    private Task CargarAsync() => _carga ??= LeerStorageAsync();

    private async Task LeerStorageAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(ClaveStorage);
            var guardada = json is null ? null : JsonSerializer.Deserialize<Sesion>(json);

            if (guardada is not null && !guardada.Vencida)
                _actual = guardada;
            else if (guardada is not null)
                SecureStorage.Default.Remove(ClaveStorage);
        }
        catch
        {
            _actual = null;
        }
    }

    private static AuthenticationState CrearEstado(Sesion? sesion)
    {
        if (sesion is null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, sesion.NombreUsuario),
            new Claim(ClaimTypes.Role, sesion.Rol)
        ], authenticationType: "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }
}

// Mismos nombres de rol que usa la API
public static class Roles
{
    public const string Admin = "Admin";
    public const string Empleado = "Empleado";
}
