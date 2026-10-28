using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Helpers;
using TiendaAPI.Models;
using TiendaAPI.Services;

namespace TiendaAPI.Controllers
{
    // Gestión de usuarios: solo Admin. El alta de usuarios sigue en POST api/auth/registro.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Rol.Admin)]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public UsuariosController(ApplicationDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private static UsuarioDto ToDto(Usuario u) => new()
        {
            Id = u.Id,
            NombreUsuario = u.NombreUsuario,
            Email = u.Email,
            RolId = u.RolId,
            RolNombre = u.Rol?.Nombre,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };

        // Id del usuario logueado (viene en el token)
        private int? UsuarioActualId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        // GET: api/usuarios?pagina=1&tamanioPagina=10&buscar=juan
        [HttpGet]
        public async Task<ActionResult<PagedResult<UsuarioDto>>> GetUsuarios([FromQuery] PaginacionParams parametros)
        {
            var query = _context.Usuarios.Include(u => u.Rol).AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(u => u.NombreUsuario.Contains(parametros.Buscar) || u.Email.Contains(parametros.Buscar));

            var resultado = await query
                .OrderBy(u => u.NombreUsuario)
                .Select(u => ToDto(u))
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(ToDto(usuario));
        }

        // PUT: api/usuarios/5  (cambiar email, rol y activo/inactivo)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarUsuario(int id, UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var rolNuevo = await _context.Roles.FindAsync(dto.RolId);
            if (rolNuevo is null) return BadRequest(new { mensaje = "El rol indicado no existe" });

            // Un admin no puede dejarse a sí mismo sin acceso
            if (id == UsuarioActualId && (!dto.Activo || rolNuevo.Nombre != Rol.Admin))
                return BadRequest(new { mensaje = "No podés quitarte el rol Admin ni desactivar tu propio usuario" });

            usuario.Email = dto.Email;
            usuario.RolId = dto.RolId;
            usuario.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("Modificar", "Usuario", usuario.Id, $"{usuario.NombreUsuario} - Rol {rolNuevo.Nombre} - Activo {usuario.Activo}");
            return NoContent();
        }

        // PUT: api/usuarios/5/password  (blanqueo de contraseña por un Admin)
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> CambiarPassword(int id, CambiarPasswordDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado" });

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("CambiarPassword", "Usuario", usuario.Id, usuario.NombreUsuario);
            return NoContent();
        }
    }
}
