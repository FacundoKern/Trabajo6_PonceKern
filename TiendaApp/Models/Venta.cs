namespace TiendaApp.Models;

public class ItemCarrito
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; }

    // Stock al momento de la última consulta a la API
    public int StockDisponible { get; set; }

    public decimal Subtotal => Precio * Cantidad;
    public bool SuperaStock => Cantidad > StockDisponible;
}

public static class MediosPago
{
    // Los mismos valores que acepta la API (PagoSimulado)
    public const string Efectivo = "Efectivo";
    public const string Tarjeta = "Tarjeta";
    public const string Transferencia = "Transferencia";
}

// POST api/salidas. El precio no se manda: lo pone la API según el producto
public class VentaRequest
{
    public int ClienteId { get; set; }
    public string? Observaciones { get; set; }
    public string MedioPago { get; set; } = MediosPago.Efectivo;
    public string? NumeroTarjeta { get; set; }
    public List<VentaDetalleRequest> Detalles { get; set; } = new();
}

public class VentaDetalleRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

// Lo que devuelve la API (SalidaDto)
public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? Observaciones { get; set; }
    public string MedioPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public string? CodigoOperacion { get; set; }
    public string? TarjetaUltimos4 { get; set; }
    public decimal Total { get; set; }
    public List<VentaDetalle> Detalles { get; set; } = new();
}

public class VentaDetalle
{
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
}
