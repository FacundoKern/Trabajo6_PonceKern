using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? Cuit { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }

    public class ProveedorCreateDto
    {
        [Required, MaxLength(100)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Cuit { get; set; }

        [MaxLength(100)]
        public string? Contacto { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }
    }
}
