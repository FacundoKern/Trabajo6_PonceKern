using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaAPI.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        // Stock actual: se actualiza automáticamente por Ingresos y Salidas
        public int Stock { get; set; } = 0;

        // Ruta relativa dentro de wwwroot/uploads, ej: "uploads/abc123.jpg"
        [MaxLength(250)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public ICollection<IngresoDetalle> IngresoDetalles { get; set; } = new List<IngresoDetalle>();
        public ICollection<SalidaDetalle> SalidaDetalles { get; set; } = new List<SalidaDetalle>();
    }
}
