using System.Security.Claims;
using TiendaAPI.Data;
using TiendaAPI.Models;

namespace TiendaAPI.Services
{
    public interface IAuditoriaService
    {
        // usuarioId: solo hace falta pasarlo cuando todavía no hay token (ej: en el login).
        // Si no se pasa, se toma del usuario logueado.
        Task RegistrarAsync(string accion, string entidad, int? entidadId = null, string? detalle = null, int? usuarioId = null);
    }

    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAsync(string accion, string entidad, int? entidadId = null, string? detalle = null, int? usuarioId = null)
        {
            var http = _httpContextAccessor.HttpContext;

            if (usuarioId is null && int.TryParse(http?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var idLogueado))
                usuarioId = idLogueado;

            _context.Auditorias.Add(new Auditoria
            {
                Fecha = DateTime.UtcNow,
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalle = detalle is { Length: > 500 } ? detalle[..500] : detalle,
                Ip = http?.Connection.RemoteIpAddress?.ToString()
            });

            await _context.SaveChangesAsync();
        }
    }
}
