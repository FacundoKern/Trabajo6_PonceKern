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

        // ---- Pago simulado ----
        // Efectivo / Tarjeta / Transferencia
        [Required, MaxLength(20)]
        public string MedioPago { get; set; } = "Efectivo";

        // Solo se guardan las ventas con pago aprobado; queda registrado por claridad
        [Required, MaxLength(20)]
        public string EstadoPago { get; set; } = "Aprobado";

        // Código que devuelve la "pasarela" simulada
        [MaxLength(40)]
        public string? CodigoOperacion { get; set; }

        // Nunca se guarda el número completo de la tarjeta, solo los últimos 4 dígitos
        [MaxLength(4)]
        public string? TarjetaUltimos4 { get; set; }

        [NotMapped]
        public decimal Total => Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;

        public ICollection<SalidaDetalle> Detalles { get; set; } = new List<SalidaDetalle>();
    }
}
