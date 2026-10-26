using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaAPI.Models
{
    // Cabecera de un ingreso de mercadería (compra a un proveedor)
    public class Ingreso
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [NotMapped]
        public decimal Total => Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;

        public ICollection<IngresoDetalle> Detalles { get; set; } = new List<IngresoDetalle>();
    }
}
