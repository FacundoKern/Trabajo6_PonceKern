using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }

    public class ClienteCreateDto
    {
        [Required, MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Dni { get; set; }

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }
    }
}
