using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaAPI.Models
{
    public class SalidaDetalle
    {
        [Key]
        public int Id { get; set; }

        public int SalidaId { get; set; }
        public Salida? Salida { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        // Precio unitario de venta al momento de la operación (histórico)
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
