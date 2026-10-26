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
    public class IngresosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IngresosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static IngresoDto ToDto(Ingreso i) => new()
        {
            Id = i.Id,
            Fecha = i.Fecha,
            ProveedorId = i.ProveedorId,
            ProveedorNombre = i.Proveedor?.RazonSocial,
            Observaciones = i.Observaciones,
            Total = i.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
            Detalles = i.Detalles.Select(d => new IngresoDetalleDto
            {
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        // GET: api/ingresos?pagina=1&tamanioPagina=10
        [HttpGet]
        public async Task<ActionResult<PagedResult<IngresoDto>>> GetIngresos([FromQuery] PaginacionParams parametros, [FromQuery] int? proveedorId)
        {
            var query = _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Detalles).ThenInclude(d => d.Producto)
                .AsQueryable();

            if (proveedorId.HasValue)
                query = query.Where(i => i.ProveedorId == proveedorId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.Fecha)
                .Skip((parametros.Pagina - 1) * parametros.TamanioPagina)
                .Take(parametros.TamanioPagina)
                .ToListAsync();

            return Ok(new PagedResult<IngresoDto>
            {
                Items = items.Select(ToDto).ToList(),
                PaginaActual = parametros.Pagina,
                TamanioPagina = parametros.TamanioPagina,
                TotalRegistros = total
            });
        }

        // GET: api/ingresos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IngresoDto>> GetIngreso(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso is null) return NotFound(new { mensaje = "Ingreso no encontrado" });

            return Ok(ToDto(ingreso));
        }

        // POST: api/ingresos
        // Registra una compra a proveedor y SUMA el stock de cada producto involucrado.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IngresoDto>> CrearIngreso(IngresoCreateDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
            if (proveedor is null || !proveedor.Activo)
                return BadRequest(new { mensaje = "El proveedor indicado no existe o está inactivo" });

            // Se usa una transacción para que la cabecera, el detalle y la actualización
            // de stock se confirmen (o se reviertan) todos juntos.
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var ingreso = new Ingreso
                {
                    ProveedorId = dto.ProveedorId,
                    Observaciones = dto.Observaciones,
                    Fecha = DateTime.UtcNow
                };

                foreach (var linea in dto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(linea.ProductoId);
                    if (producto is null)
                        return BadRequest(new { mensaje = $"El producto {linea.ProductoId} no existe" });

                    producto.Stock += linea.Cantidad; // ingreso siempre suma stock

                    ingreso.Detalles.Add(new IngresoDetalle
                    {
                        ProductoId = linea.ProductoId,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = linea.PrecioUnitario
                    });
                }

                _context.Ingresos.Add(ingreso);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                await _context.Entry(ingreso).Reference(i => i.Proveedor).LoadAsync();
                foreach (var d in ingreso.Detalles)
                    await _context.Entry(d).Reference(x => x.Producto).LoadAsync();

                return CreatedAtAction(nameof(GetIngreso), new { id = ingreso.Id }, ToDto(ingreso));
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
