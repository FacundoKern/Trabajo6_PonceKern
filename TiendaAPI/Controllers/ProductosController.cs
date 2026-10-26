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
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanioMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ProductosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private static ProductoDto ToDto(Producto p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            ImagenUrl = p.ImagenUrl,
            Activo = p.Activo,
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre
        };

        // GET: api/productos?pagina=1&tamanioPagina=10&buscar=mouse&categoriaId=2
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductoDto>>> GetProductos(
            [FromQuery] PaginacionParams parametros, [FromQuery] int? categoriaId)
        {
            var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(p => p.Nombre.Contains(parametros.Buscar));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Nombre)
                .Skip((parametros.Pagina - 1) * parametros.TamanioPagina)
                .Take(parametros.TamanioPagina)
                .Select(p => ToDto(p))
                .ToListAsync();

            return Ok(new PagedResult<ProductoDto>
            {
                Items = items,
                PaginaActual = parametros.Pagina,
                TamanioPagina = parametros.TamanioPagina,
                TotalRegistros = total
            });
        }

        // GET: api/productos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
            if (producto is null) return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(ToDto(producto));
        }

        // POST: api/productos
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductoDto>> CrearProducto(ProductoCreateDto dto)
        {
            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste) return BadRequest(new { mensaje = "La categoría indicada no existe" });

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                CategoriaId = dto.CategoriaId,
                Stock = 0 // el stock siempre arranca en 0; se carga mediante un Ingreso
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, ToDto(producto));
        }

        // PUT: api/productos/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarProducto(int id, ProductoUpdateDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto is null) return NotFound(new { mensaje = "Producto no encontrado" });

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste) return BadRequest(new { mensaje = "La categoría indicada no existe" });

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.CategoriaId = dto.CategoriaId;
            producto.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/productos/5  (baja lógica, para no romper historial de ingresos/salidas)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto is null) return NotFound(new { mensaje = "Producto no encontrado" });

            producto.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/productos/5/imagen
        // Sube UNA imagen asociada al producto, se guarda en wwwroot/uploads
        [HttpPost("{id:int}/imagen")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(TamanioMaximoBytes)]
        public async Task<ActionResult> SubirImagen(int id, IFormFile archivo)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto is null) return NotFound(new { mensaje = "Producto no encontrado" });

            if (archivo is null || archivo.Length == 0)
                return BadRequest(new { mensaje = "Debe adjuntar un archivo" });

            if (archivo.Length > TamanioMaximoBytes)
                return BadRequest(new { mensaje = "La imagen no puede superar los 5 MB" });

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
                return BadRequest(new { mensaje = "Formato no permitido. Use jpg, jpeg, png o webp" });

            var carpetaUploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(carpetaUploads);

            // Si ya tenía una imagen previa, se elimina del disco
            if (!string.IsNullOrEmpty(producto.ImagenUrl))
            {
                var rutaAnterior = Path.Combine(_env.WebRootPath ?? "wwwroot", producto.ImagenUrl);
                if (System.IO.File.Exists(rutaAnterior)) System.IO.File.Delete(rutaAnterior);
            }

            var nombreArchivo = $"producto_{id}_{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpetaUploads, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            producto.ImagenUrl = $"uploads/{nombreArchivo}";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Imagen cargada correctamente", producto.ImagenUrl });
        }
    }
}
