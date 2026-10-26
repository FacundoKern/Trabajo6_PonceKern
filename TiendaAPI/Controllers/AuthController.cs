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

        public AuthController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario && u.Activo);

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });

            var (token, expira) = _tokenService.GenerarToken(usuario);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expira = expira,
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
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

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                Rol = dto.Rol == "Admin" ? "Admin" : "Empleado",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { mensaje = "Usuario creado correctamente", usuario.Id });
        }
    }
}
