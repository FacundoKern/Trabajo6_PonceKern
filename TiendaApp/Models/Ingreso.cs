namespace TiendaApp.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? Cuit { get; set; }
    public bool Activo { get; set; }
}

// POST api/ingresos. Acá sí va el precio: es el costo que se le pagó al proveedor
public class IngresoRequest
{
    public int ProveedorId { get; set; }
    public string? Observaciones { get; set; }
    public List<IngresoDetalleRequest> Detalles { get; set; } = new();
}

public class IngresoDetalleRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

// Lo que devuelve la API (IngresoDto)
public class Ingreso
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public string? Observaciones { get; set; }
    public decimal Total { get; set; }
    public List<IngresoDetalle> Detalles { get; set; } = new();
}

public class IngresoDetalle
{
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
