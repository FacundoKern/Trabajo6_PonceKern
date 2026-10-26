using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Helpers;
using TiendaAPI.Models;

namespace TiendaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static ProveedorDto ToDto(Proveedor p) => new()
        {
            Id = p.Id,
            RazonSocial = p.RazonSocial,
            Cuit = p.Cuit,
            Contacto = p.Contacto,
            Telefono = p.Telefono,
            Email = p.Email,
            Direccion = p.Direccion,
            Activo = p.Activo
        };

        // GET: api/proveedores?pagina=1&tamanioPagina=10&buscar=acme
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProveedorDto>>> GetProveedores([FromQuery] PaginacionParams parametros)
        {
            var query = _context.Proveedores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(p => p.RazonSocial.Contains(parametros.Buscar));

            var resultado = await query
                .OrderBy(p => p.RazonSocial)
                .Select(p => ToDto(p))
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/proveedores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound(new { mensaje = "Proveedor no encontrado" });

            return Ok(ToDto(proveedor));
        }

        // POST: api/proveedores
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProveedorDto>> CrearProveedor(ProveedorCreateDto dto)
        {
            var proveedor = new Proveedor
            {
                RazonSocial = dto.RazonSocial,
                Cuit = dto.Cuit,
                Contacto = dto.Contacto,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, ToDto(proveedor));
        }

        // PUT: api/proveedores/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarProveedor(int id, ProveedorCreateDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound(new { mensaje = "Proveedor no encontrado" });

            proveedor.RazonSocial = dto.RazonSocial;
            proveedor.Cuit = dto.Cuit;
            proveedor.Contacto = dto.Contacto;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/proveedores/5 (baja lógica para no romper historial de ingresos)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound(new { mensaje = "Proveedor no encontrado" });

            proveedor.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
