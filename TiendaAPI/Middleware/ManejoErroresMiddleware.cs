using System.Security.Claims;
using TiendaAPI.Data;
using TiendaAPI.Models;

//capa de software que actúa como un puente entre diferentes aplicaciones, bases de datos, sistemas operativos o redes,
//facilitando que se comuniquen y compartan datos de manera eficiente.

namespace TiendaAPI.Middleware
{
    // Atrapa cualquier excepción no controlada de la API:
    //  1) la guarda en la tabla LogErrores (y en el log de la consola)
    //  2) responde un 500 en JSON con un mensaje amigable y el Id del error,
    //     en vez de la página de error por defecto (que expone detalles internos).
    public class ManejoErroresMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ManejoErroresMiddleware> _logger;

        public ManejoErroresMiddleware(RequestDelegate next, ILogger<ManejoErroresMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

                var errorId = await GuardarEnBaseAsync(ex, context, scopeFactory);

                if (context.Response.HasStarted) throw; // ya se mandó parte de la respuesta, no se puede reemplazar

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "Ocurrió un error inesperado. Si el problema persiste, informe el código de error.",
                    errorId
                });
            }
        }

        // Usa un DbContext nuevo: el del request puede haber quedado en un estado inválido por el error
        private async Task<int?> GuardarEnBaseAsync(Exception ex, HttpContext context, IServiceScopeFactory scopeFactory)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var log = new LogError
                {
                    Fecha = DateTime.UtcNow,
                    Mensaje = Recortar(ex.Message, 500),
                    TipoExcepcion = Recortar(ex.GetType().FullName ?? "", 200),
                    StackTrace = ex.ToString(),
                    Metodo = context.Request.Method,
                    Ruta = Recortar(context.Request.Path + context.Request.QueryString, 300),
                    UsuarioId = int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null
                };

                db.LogErrores.Add(log);
                await db.SaveChangesAsync();
                return log.Id;
            }
            catch (Exception exLog)
            {
                // Si tampoco se puede guardar (ej: la base está caída) queda al menos en el log de consola
                _logger.LogError(exLog, "No se pudo guardar el error en la tabla LogErrores");
                return null;
            }
        }

        private static string Recortar(string texto, int largo) =>
            texto.Length <= largo ? texto : texto[..largo];
    }
}
