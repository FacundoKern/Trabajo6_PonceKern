using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Helpers;
using TiendaAPI.Models;

namespace TiendaAPI.Controllers
{
    // Consulta de auditoría y del log de errores: solo Admin.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Rol.Admin)]
    public class AuditoriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/auditoria?pagina=1&tamanioPagina=20&buscar=Venta&usuarioId=1&desde=2026-10-01&hasta=2026-10-31
        // buscar filtra por acción, entidad o detalle. Las fechas son en hora argentina.
        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditoriaDto>>> GetAuditoria(
            [FromQuery] PaginacionParams parametros, [FromQuery] int? usuarioId,
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var query = _context.Auditorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(a => a.Accion.Contains(parametros.Buscar)
                    || a.Entidad.Contains(parametros.Buscar)
                    || (a.Detalle != null && a.Detalle.Contains(parametros.Buscar)));

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (desde.HasValue)
            {
                var desdeUtc = FechaHelper.InicioDiaUtc(desde.Value);
                query = query.Where(a => a.Fecha >= desdeUtc);
            }

            if (hasta.HasValue)
            {
                var hastaUtc = FechaHelper.FinDiaUtc(hasta.Value);
                query = query.Where(a => a.Fecha < hastaUtc);
            }

            var resultado = await query
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    UsuarioId = a.UsuarioId,
                    NombreUsuario = a.Usuario != null ? a.Usuario.NombreUsuario : null,
                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    Detalle = a.Detalle,
                    Ip = a.Ip
                })
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/auditoria/errores?pagina=1&tamanioPagina=20&buscar=NullReference
        [HttpGet("errores")]
        public async Task<ActionResult<PagedResult<LogErrorDto>>> GetErrores([FromQuery] PaginacionParams parametros)
        {
            var query = _context.LogErrores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(l => l.Mensaje.Contains(parametros.Buscar)
                    || (l.Ruta != null && l.Ruta.Contains(parametros.Buscar)));

            var resultado = await query
                .OrderByDescending(l => l.Fecha)
                .Select(l => new LogErrorDto
                {
                    Id = l.Id,
                    Fecha = l.Fecha,
                    Mensaje = l.Mensaje,
                    TipoExcepcion = l.TipoExcepcion,
                    Metodo = l.Metodo,
                    Ruta = l.Ruta,
                    NombreUsuario = l.Usuario != null ? l.Usuario.NombreUsuario : null
                })
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/auditoria/errores/5  (incluye el detalle técnico completo)
        [HttpGet("errores/{id:int}")]
        public async Task<ActionResult<LogErrorDto>> GetError(int id)
        {
            var error = await _context.LogErrores
                .Where(l => l.Id == id)
                .Select(l => new LogErrorDto
                {
                    Id = l.Id,
                    Fecha = l.Fecha,
                    Mensaje = l.Mensaje,
                    TipoExcepcion = l.TipoExcepcion,
                    Metodo = l.Metodo,
                    Ruta = l.Ruta,
                    NombreUsuario = l.Usuario != null ? l.Usuario.NombreUsuario : null,
                    StackTrace = l.StackTrace
                })
                .FirstOrDefaultAsync();

            if (error is null) return NotFound(new { mensaje = "Error no encontrado" });
            return Ok(error);
        }

        // POST: api/auditoria/probar-error
        // Provoca un error a propósito para demostrar que el middleware lo registra en LogErrores.
        [HttpPost("probar-error")]
        public IActionResult ProbarError()
        {
            throw new InvalidOperationException("Error de prueba generado desde api/auditoria/probar-error");
        }
    }
}
