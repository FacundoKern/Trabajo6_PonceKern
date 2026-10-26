using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class IngresoDetalleCreateDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PrecioUnitario { get; set; }
    }

    public class IngresoCreateDto
    {
        [Required]
        public int ProveedorId { get; set; }

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [Required, MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<IngresoDetalleCreateDto> Detalles { get; set; } = new();
    }

    public class IngresoDetalleDto
    {
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class IngresoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public string? Observaciones { get; set; }
        public decimal Total { get; set; }
        public List<IngresoDetalleDto> Detalles { get; set; } = new();
    }
}
