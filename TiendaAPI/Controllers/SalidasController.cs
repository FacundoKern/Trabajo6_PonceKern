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
    public class SalidasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalidasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static SalidaDto ToDto(Salida s) => new()
        {
            Id = s.Id,
            Fecha = s.Fecha,
            ClienteId = s.ClienteId,
            ClienteNombre = s.Cliente is null ? null : $"{s.Cliente.Nombre} {s.Cliente.Apellido}",
            Observaciones = s.Observaciones,
            Total = s.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
            Detalles = s.Detalles.Select(d => new SalidaDetalleDto
            {
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        // GET: api/salidas?pagina=1&tamanioPagina=10
        [HttpGet]
        public async Task<ActionResult<PagedResult<SalidaDto>>> GetSalidas([FromQuery] PaginacionParams parametros, [FromQuery] int? clienteId)
        {
            var query = _context.Salidas
                .Include(s => s.Cliente)
                .Include(s => s.Detalles).ThenInclude(d => d.Producto)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(s => s.ClienteId == clienteId.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.Fecha)
                .Skip((parametros.Pagina - 1) * parametros.TamanioPagina)
                .Take(parametros.TamanioPagina)
                .ToListAsync();

            return Ok(new PagedResult<SalidaDto>
            {
                Items = items.Select(ToDto).ToList(),
                PaginaActual = parametros.Pagina,
                TamanioPagina = parametros.TamanioPagina,
                TotalRegistros = total
            });
        }

        // GET: api/salidas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SalidaDto>> GetSalida(int id)
        {
            var salida = await _context.Salidas
                .Include(s => s.Cliente)
                .Include(s => s.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salida is null) return NotFound(new { mensaje = "Salida no encontrada" });

            return Ok(ToDto(salida));
        }

        // POST: api/salidas
        // Registra una venta a cliente y RESTA el stock. No permite vender sin stock suficiente.
        [HttpPost]
        public async Task<ActionResult<SalidaDto>> CrearSalida(SalidaCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente is null || !cliente.Activo)
                return BadRequest(new { mensaje = "El cliente indicado no existe o está inactivo" });

            // Se agrupan cantidades por producto por si el mismo producto aparece en más de una línea,
            // así la validación de stock contempla el total pedido y no solo la línea individual.
            var cantidadPorProducto = dto.Detalles
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Validar stock de TODOS los productos antes de descontar nada
                var errores = new List<string>();
                var productos = new Dictionary<int, Producto>();

                foreach (var (productoId, cantidadPedida) in cantidadPorProducto)
                {
                    var producto = await _context.Productos.FindAsync(productoId);
                    if (producto is null || !producto.Activo)
                    {
                        errores.Add($"El producto {productoId} no existe o está inactivo");
                        continue;
                    }

                    if (producto.Stock < cantidadPedida)
                    {
                        errores.Add($"Stock insuficiente para '{producto.Nombre}' (disponible: {producto.Stock}, solicitado: {cantidadPedida})");
                        continue;
                    }

                    productos[productoId] = producto;
                }

                if (errores.Count > 0)
                {
                    await transaccion.RollbackAsync();
                    return BadRequest(new { mensaje = "No se pudo registrar la venta", errores });
                }

                // 2) Descontar stock y armar la venta
                var salida = new Salida
                {
                    ClienteId = dto.ClienteId,
                    Observaciones = dto.Observaciones,
                    Fecha = DateTime.UtcNow
                };

                foreach (var linea in dto.Detalles)
                {
                    productos[linea.ProductoId].Stock -= linea.Cantidad;

                    salida.Detalles.Add(new SalidaDetalle
                    {
                        ProductoId = linea.ProductoId,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = linea.PrecioUnitario
                    });
                }

                _context.Salidas.Add(salida);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                await _context.Entry(salida).Reference(s => s.Cliente).LoadAsync();
                foreach (var d in salida.Detalles)
                    await _context.Entry(d).Reference(x => x.Producto).LoadAsync();

                return CreatedAtAction(nameof(GetSalida), new { id = salida.Id }, ToDto(salida));
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
