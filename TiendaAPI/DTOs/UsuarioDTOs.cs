using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string? RolNombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class UsuarioUpdateDto
    {
        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class CambiarPasswordDto
    {
        [Required, MinLength(6)]
        public string NuevaPassword { get; set; } = string.Empty;
    }

    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CantidadUsuarios { get; set; }
    }

    public class RolCreateDto
    {
        [Required, MaxLength(30)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Descripcion { get; set; }
    }
}
