using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Models;
using TiendaAPI.Services;

namespace TiendaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IAuditoriaService _auditoria;

        public AuthController(ApplicationDbContext context, ITokenService tokenService, IAuditoriaService auditoria)
        {
            _context = context;
            _tokenService = tokenService;
            _auditoria = auditoria;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario && u.Activo);

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                await _auditoria.RegistrarAsync("LoginFallido", "Usuario", detalle: $"Intento con el usuario '{dto.NombreUsuario}'");
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
            }

            var (token, expira) = _tokenService.GenerarToken(usuario);
            await _auditoria.RegistrarAsync("Login", "Usuario", usuario.Id, usuarioId: usuario.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expira = expira,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol!.Nombre
            });
        }

        // POST: api/auth/registro
        // Solo un Admin autenticado puede registrar nuevos usuarios.
        [HttpPost("registro")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Registro(RegistroDto dto)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario);
            if (existe)
                return Conflict(new { mensaje = "Ya existe un usuario con ese nombre" });

            // Si no se indica rol, o no existe, queda como Empleado
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == dto.Rol)
                ?? await _context.Roles.FirstAsync(r => r.Nombre == Rol.Empleado);

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                RolId = rol.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("Crear", "Usuario", usuario.Id, $"{usuario.NombreUsuario} con rol {rol.Nombre}");

            return StatusCode(201, new { mensaje = "Usuario creado correctamente", usuario.Id });
        }
    }
}
