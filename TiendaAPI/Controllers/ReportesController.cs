using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.Helpers;
using TiendaAPI.Models;
using TiendaAPI.Services;
using static TiendaAPI.Services.ReportePdf;

namespace TiendaAPI.Controllers
{
    // Reportes en PDF. La app los descarga, los muestra y los comparte por mail.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public ReportesController(ApplicationDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private FileContentResult Pdf(byte[] contenido, string nombre) =>
            File(contenido, "application/pdf", $"{nombre}_{FechaHelper.ALocal(DateTime.UtcNow):yyyyMMdd_HHmm}.pdf");

        // GET: api/reportes/stock?categoriaId=1&stockMinimo=5
        // Stock actual de los productos activos. Con stockMinimo marca los que están por debajo.
        [HttpGet("stock")]
        public async Task<IActionResult> Stock([FromQuery] int? categoriaId, [FromQuery] int stockMinimo = 5)
        {
            var query = _context.Productos.Include(p => p.Categoria).Where(p => p.Activo);
            if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var productos = await query.OrderBy(p => p.Categoria!.Nombre).ThenBy(p => p.Nombre).ToListAsync();

            var columnas = new List<Columna>
            {
                new("Producto", 3), new("Categoría", 2), new("Precio", 1.3f, true),
                new("Stock", 1, true), new("Valorizado", 1.5f, true), new("Estado", 1.2f)
            };

            var filas = productos.Select(p => new[]
            {
                p.Nombre,
                p.Categoria?.Nombre ?? "-",
                Moneda(p.Precio),
                p.Stock.ToString(),
                Moneda(p.Precio * p.Stock),
                p.Stock == 0 ? "SIN STOCK" : p.Stock < stockMinimo ? "Stock bajo" : "OK"
            });

            var resumen = new[]
            {
                $"Productos: {productos.Count}  -  Sin stock: {productos.Count(p => p.Stock == 0)}  -  Stock bajo (< {stockMinimo}): {productos.Count(p => p.Stock > 0 && p.Stock < stockMinimo)}",
                $"Unidades totales: {productos.Sum(p => p.Stock)}",
                $"Valor total del inventario: {Moneda(productos.Sum(p => p.Precio * p.Stock))}"
            };

            var pdf = Generar("Reporte de stock", "Productos activos con su stock actual", columnas, filas, resumen);
            await _auditoria.RegistrarAsync("Reporte", "Stock");
            return Pdf(pdf, "reporte_stock");
        }
    }
}
