using System.Text.Json;
using TiendaApp.Models;

namespace TiendaApp.Services;

// Carrito de compras. Se guarda en el teléfono (Preferences) para no perderlo al cerrar la app.
public class CarritoService
{
    private const string ClaveStorage = "carrito";

    private readonly CatalogoService _catalogo;
    private readonly List<ItemCarrito> _items;

    // Avisa a las pantallas (por ejemplo, el ícono del carrito) que algo cambió
    public event Action? Cambio;

    public CarritoService(CatalogoService catalogo)
    {
        _catalogo = catalogo;
        _items = Leer();
    }

    public IReadOnlyList<ItemCarrito> Items => _items;
    public int CantidadTotal => _items.Sum(i => i.Cantidad);
    public decimal Total => _items.Sum(i => i.Subtotal);
    public bool HayProblemasDeStock => _items.Any(i => i.SuperaStock);

    public int CantidadDe(int productoId) =>
        _items.FirstOrDefault(i => i.ProductoId == productoId)?.Cantidad ?? 0;

    // Devuelve cuántas unidades se agregaron de verdad (nunca más que el stock)
    public int Agregar(Producto producto, int cantidad)
    {
        var item = _items.FirstOrDefault(i => i.ProductoId == producto.Id);
        if (item is null)
        {
            item = new ItemCarrito { ProductoId = producto.Id };
            _items.Add(item);
        }

        var anterior = item.Cantidad;
        item.Nombre = producto.Nombre;
        item.Precio = producto.Precio;
        item.ImagenUrl = producto.ImagenUrl;
        item.StockDisponible = producto.Stock;
        item.Cantidad = Math.Min(anterior + cantidad, producto.Stock);

        if (item.Cantidad <= 0)
            _items.Remove(item);

        Guardar();
        return item.Cantidad - anterior;
    }

    public void CambiarCantidad(int productoId, int cantidad)
    {
        var item = _items.FirstOrDefault(i => i.ProductoId == productoId);
        if (item is null) return;

        if (cantidad <= 0)
            _items.Remove(item);
        else
            item.Cantidad = Math.Min(cantidad, item.StockDisponible);

        Guardar();
    }

    public void Quitar(int productoId) => CambiarCantidad(productoId, 0);

    public void Vaciar()
    {
        _items.Clear();
        Guardar();
    }

    // Antes de pagar se vuelven a pedir precio y stock: pueden haber cambiado desde que se agregó
    public async Task ActualizarDesdeApiAsync()
    {
        var consultas = _items.Select(async item => (item, producto: await _catalogo.ObtenerProductoAsync(item.ProductoId)));

        foreach (var (item, producto) in await Task.WhenAll(consultas))
        {
            if (producto is null || !producto.Activo)
            {
                item.StockDisponible = 0;
                continue;
            }
            item.Nombre = producto.Nombre;
            item.Precio = producto.Precio;
            item.ImagenUrl = producto.ImagenUrl;
            item.StockDisponible = producto.Stock;
        }

        Guardar();
    }

    private void Guardar()
    {
        try
        {
            Preferences.Default.Set(ClaveStorage, JsonSerializer.Serialize(_items));
        }
        catch
        {
            // Si no se puede guardar, el carrito sigue funcionando en memoria
        }
        Cambio?.Invoke();
    }

    private static List<ItemCarrito> Leer()
    {
        try
        {
            var json = Preferences.Default.Get<string?>(ClaveStorage, null);
            return json is null ? new() : JsonSerializer.Deserialize<List<ItemCarrito>>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }
}
