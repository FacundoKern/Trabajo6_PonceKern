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
    [Authorize] // requiere JWT para todo el controlador
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias?pagina=1&tamanioPagina=10&buscar=bebida
        [HttpGet]
        public async Task<ActionResult<PagedResult<CategoriaDto>>> GetCategorias([FromQuery] PaginacionParams parametros)
        {
            var query = _context.Categorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(c => c.Nombre.Contains(parametros.Buscar));

            var resultado = await query
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, Descripcion = c.Descripcion })
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/categorias/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria is null) return NotFound(new { mensaje = "Categoría no encontrada" });

            return Ok(new CategoriaDto { Id = categoria.Id, Nombre = categoria.Nombre, Descripcion = categoria.Descripcion });
        }

        // POST: api/categorias
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoriaDto>> CrearCategoria(CategoriaCreateDto dto)
        {
            var categoria = new Categoria { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var resultado = new CategoriaDto { Id = categoria.Id, Nombre = categoria.Nombre, Descripcion = categoria.Descripcion };
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, resultado);
        }

        // PUT: api/categorias/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarCategoria(int id, CategoriaCreateDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria is null) return NotFound(new { mensaje = "Categoría no encontrada" });

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categorias/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria is null) return NotFound(new { mensaje = "Categoría no encontrada" });

            var tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (tieneProductos)
                return BadRequest(new { mensaje = "No se puede eliminar: hay productos asociados a esta categoría" });

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
