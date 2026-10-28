using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TiendaApp.Services;

// Convierte las respuestas de error de la API en un texto para mostrar en pantalla
public static class ApiErrores
{
    public const string SinConexion = "No se pudo conectar con el servidor. Revisá la conexión a internet.";

    // Manda el pedido y devuelve null si salió bien, o el mensaje de error
    public static async Task<string?> EnviarAsync(Func<Task<HttpResponseMessage>> envio)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await envio();
        }
        catch (Exception)
        {
            return SinConexion;
        }

        return respuesta.IsSuccessStatusCode ? null : await LeerAsync(respuesta);
    }

    // Igual que EnviarAsync, pero cuando sale bien devuelve el objeto que respondió la API
    public static async Task<(T? Valor, string? Error)> EnviarAsync<T>(Func<Task<HttpResponseMessage>> envio)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await envio();
        }
        catch (Exception)
        {
            return (default, SinConexion);
        }

        if (respuesta.IsSuccessStatusCode)
            return (await respuesta.Content.ReadFromJsonAsync<T>(), null);

        return (default, await LeerAsync(respuesta));
    }

    // La API responde { mensaje, errores: [...] }, o { errors: { Campo: [...] } } si falla la validación del modelo
    public static async Task<string> LeerAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            return "No tenés permiso para hacer esto.";

        try
        {
            using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            var raiz = json.RootElement;

            if (raiz.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Array && errores.GetArrayLength() > 0)
                return string.Join("\n", errores.EnumerateArray().Select(e => e.GetString()));

            if (raiz.TryGetProperty("mensaje", out var mensaje))
                return mensaje.GetString() ?? "Error desconocido";

            if (raiz.TryGetProperty("errors", out var validacion) && validacion.ValueKind == JsonValueKind.Object)
                return string.Join("\n", validacion.EnumerateObject()
                    .SelectMany(campo => campo.Value.EnumerateArray().Select(e => e.GetString())));
        }
        catch (JsonException)
        {
        }
        return $"El servidor respondió con un error (HTTP {(int)respuesta.StatusCode}).";
    }
}
