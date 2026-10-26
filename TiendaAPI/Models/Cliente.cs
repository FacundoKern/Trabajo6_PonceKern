using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Dni { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Salida> Salidas { get; set; } = new List<Salida>();
    }
}
