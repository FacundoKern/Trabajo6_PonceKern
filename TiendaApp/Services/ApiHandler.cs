using System.Net;
using System.Net.Http.Headers;

namespace TiendaApp.Services;

// Se ejecuta en cada pedido a la API: agrega el token y, si la API lo rechaza
// (vencido, o el servidor cambió la clave JWT), cierra la sesión para volver al login.
public class ApiHandler : DelegatingHandler
{
    private readonly SesionService _sesion;

    public ApiHandler(SesionService sesion) : base(new HttpClientHandler())
    {
        _sesion = sesion;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _sesion.ObtenerTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await base.SendAsync(request, cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized && token is not null)
            await _sesion.CerrarSesionAsync();

        return respuesta;
    }
}
