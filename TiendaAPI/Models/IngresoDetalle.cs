using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaAPI.Models
{
    public class IngresoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int IngresoId { get; set; }
        public Ingreso? Ingreso { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        // Precio unitario de compra al momento del ingreso (histórico, no se recalcula si cambia el precio del producto)
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
