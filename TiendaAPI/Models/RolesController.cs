using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Models;

namespace TiendaAPI.Controllers
{
    // Gestión de roles: solo Admin.
    // Los roles base (Admin y Empleado) no se pueden renombrar ni borrar porque los usa
    // [Authorize(Roles = ...)] en los controladores.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Rol.Admin)]
    public class RolesController : ControllerBase
    {
        private static readonly string[] RolesBase = { Rol.Admin, Rol.Empleado };

        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roles  (son pocos, no hace falta paginar)
        [HttpGet]
        public async Task<ActionResult<List<RolDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Id)
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    CantidadUsuarios = r.Usuarios.Count
                })
                .ToListAsync();

            return Ok(roles);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RolDto>> CrearRol(RolCreateDto dto)
        {
            if (await _context.Roles.AnyAsync(r => r.Nombre == dto.Nombre))
                return Conflict(new { mensaje = "Ya existe un rol con ese nombre" });

            var rol = new Rol { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return StatusCode(201, new RolDto { Id = rol.Id, Nombre = rol.Nombre, Descripcion = rol.Descripcion });
        }

        // PUT: api/roles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarRol(int id, RolCreateDto dto)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol is null) return NotFound(new { mensaje = "Rol no encontrado" });

            if (RolesBase.Contains(rol.Nombre) && rol.Nombre != dto.Nombre)
                return BadRequest(new { mensaje = "Los roles Admin y Empleado no se pueden renombrar" });

            if (await _context.Roles.AnyAsync(r => r.Nombre == dto.Nombre && r.Id != id))
                return Conflict(new { mensaje = "Ya existe un rol con ese nombre" });

            rol.Nombre = dto.Nombre;
            rol.Descripcion = dto.Descripcion;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/roles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol is null) return NotFound(new { mensaje = "Rol no encontrado" });

            if (RolesBase.Contains(rol.Nombre))
                return BadRequest(new { mensaje = "Los roles Admin y Empleado no se pueden eliminar" });

            if (await _context.Usuarios.AnyAsync(u => u.RolId == id))
                return BadRequest(new { mensaje = "No se puede eliminar: hay usuarios con este rol" });

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
