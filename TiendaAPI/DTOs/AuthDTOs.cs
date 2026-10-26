using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class LoginDto
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegistroDto
    {
        [Required, MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Solo un usuario Admin autenticado puede crear otro Admin (ver AuthController)
        public string Rol { get; set; } = "Empleado";
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
