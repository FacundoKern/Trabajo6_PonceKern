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
    // Las fechas desde/hasta son en hora argentina; si no se mandan, se toman los últimos 30 días.
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

        private static (DateTime Desde, DateTime Hasta) RangoPorDefecto(DateTime? desde, DateTime? hasta)
        {
            var hoy = FechaHelper.ALocal(DateTime.UtcNow).Date;
            return ((desde ?? hoy.AddDays(-30)).Date, (hasta ?? hoy).Date);
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

        // GET: api/reportes/ingresos?desde=2026-10-01&hasta=2026-10-31  (entradas de mercadería)
        [HttpGet("ingresos")]
        [Authorize(Roles = Rol.Admin)]
        public async Task<IActionResult> Ingresos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = RangoPorDefecto(desde, hasta);
            var desdeUtc = FechaHelper.InicioDiaUtc(d);
            var hastaUtc = FechaHelper.FinDiaUtc(h);

            var lineas = await _context.IngresoDetalles
                .Include(x => x.Ingreso).ThenInclude(i => i!.Proveedor)
                .Include(x => x.Producto)
                .Where(x => x.Ingreso!.Fecha >= desdeUtc && x.Ingreso.Fecha < hastaUtc)
                .OrderBy(x => x.Ingreso!.Fecha).ThenBy(x => x.IngresoId)
                .ToListAsync();

            var columnas = new List<Columna>
            {
                new("Fecha", 2f), new("N°", 0.6f, true), new("Proveedor", 2.2f), new("Producto", 2.5f),
                new("Cant.", 0.8f, true), new("P. unit.", 1.3f, true), new("Subtotal", 1.4f, true)
            };

            var filas = lineas.Select(x => new[]
            {
                Fecha(x.Ingreso!.Fecha),
                x.IngresoId.ToString(),
                x.Ingreso.Proveedor?.RazonSocial ?? "-",
                x.Producto?.Nombre ?? "-",
                x.Cantidad.ToString(),
                Moneda(x.PrecioUnitario),
                Moneda(x.Cantidad * x.PrecioUnitario)
            });

            var resumen = new[]
            {
                $"Ingresos: {lineas.Select(x => x.IngresoId).Distinct().Count()}  -  Unidades ingresadas: {lineas.Sum(x => x.Cantidad)}",
                $"Total comprado: {Moneda(lineas.Sum(x => x.Cantidad * x.PrecioUnitario))}"
            };

            var pdf = Generar("Reporte de entradas de mercadería", $"Compras a proveedores del {d:dd/MM/yyyy} al {h:dd/MM/yyyy}", columnas, filas, resumen);
            await _auditoria.RegistrarAsync("Reporte", "Ingresos", detalle: $"{d:dd/MM/yyyy} al {h:dd/MM/yyyy}");
            return Pdf(pdf, "reporte_ingresos");
        }

        // GET: api/reportes/ventas?desde=2026-10-01&hasta=2026-10-31  (salidas de mercadería)
        [HttpGet("ventas")]
        [Authorize(Roles = Rol.Admin)]
        public async Task<IActionResult> Ventas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d, h) = RangoPorDefecto(desde, hasta);
            var desdeUtc = FechaHelper.InicioDiaUtc(d);
            var hastaUtc = FechaHelper.FinDiaUtc(h);

            var lineas = await _context.SalidaDetalles
                .Include(x => x.Salida).ThenInclude(s => s!.Cliente)
                .Include(x => x.Producto)
                .Where(x => x.Salida!.Fecha >= desdeUtc && x.Salida.Fecha < hastaUtc)
                .OrderBy(x => x.Salida!.Fecha).ThenBy(x => x.SalidaId)
                .ToListAsync();

            var columnas = new List<Columna>
            {
                new("Fecha", 2f), new("N°", 0.6f, true), new("Cliente", 2f), new("Producto", 2.5f),
                new("Cant.", 0.8f, true), new("P. unit.", 1.3f, true), new("Subtotal", 1.4f, true), new("Pago", 1.3f)
            };

            var filas = lineas.Select(x => new[]
            {
                Fecha(x.Salida!.Fecha),
                x.SalidaId.ToString(),
                x.Salida.Cliente is null ? "-" : $"{x.Salida.Cliente.Nombre} {x.Salida.Cliente.Apellido}",
                x.Producto?.Nombre ?? "-",
                x.Cantidad.ToString(),
                Moneda(x.PrecioUnitario),
                Moneda(x.Cantidad * x.PrecioUnitario),
                x.Salida.MedioPago
            });

            // Total vendido por cada medio de pago
            var porMedio = lineas
                .GroupBy(x => x.Salida!.MedioPago)
                .Select(g => $"{g.Key}: {Moneda(g.Sum(x => x.Cantidad * x.PrecioUnitario))}");

            var resumen = new List<string>
            {
                $"Ventas: {lineas.Select(x => x.SalidaId).Distinct().Count()}  -  Unidades vendidas: {lineas.Sum(x => x.Cantidad)}",
                string.Join("  -  ", porMedio),
                $"Total vendido: {Moneda(lineas.Sum(x => x.Cantidad * x.PrecioUnitario))}"
            };

            var pdf = Generar("Reporte de ventas", $"Salidas de mercadería del {d:dd/MM/yyyy} al {h:dd/MM/yyyy}", columnas, filas, resumen);
            await _auditoria.RegistrarAsync("Reporte", "Ventas", detalle: $"{d:dd/MM/yyyy} al {h:dd/MM/yyyy}");
            return Pdf(pdf, "reporte_ventas");
        }
    }
}
