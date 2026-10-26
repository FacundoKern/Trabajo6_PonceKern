using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaAPI.Models
{
    // Cabecera de una salida de mercadería (venta a un cliente)
    public class Salida
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [MaxLength(250)]
        public string? Observaciones { get; set; }

        [NotMapped]
        public decimal Total => Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;

        public ICollection<SalidaDetalle> Detalles { get; set; } = new List<SalidaDetalle>();
    }
}
