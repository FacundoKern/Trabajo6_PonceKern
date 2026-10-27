using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class SalidaDetalleCreateDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        // El precio NO se recibe del cliente: la API usa el precio actual del producto,
        // así nadie puede vender a un precio inventado desde la app.
    }

    public class SalidaCreateDto
    {
        [Required]
        public int ClienteId { get; set; }

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [Required, MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<SalidaDetalleCreateDto> Detalles { get; set; } = new();
    }

    public class SalidaDetalleDto
    {
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class SalidaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? Observaciones { get; set; }
        public decimal Total { get; set; }
        public List<SalidaDetalleDto> Detalles { get; set; } = new();
    }
}
