using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.Models
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Cuit { get; set; }

        [MaxLength(100)]
        public string? Contacto { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}
