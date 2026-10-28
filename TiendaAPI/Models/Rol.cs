using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.Models
{
    // Roles de usuario en tabla propia (antes era un texto suelto dentro de Usuarios).
    // "Admin" y "Empleado" son roles base: los usa [Authorize(Roles = ...)] en los controladores.
    public class Rol
    {
        public const string Admin = "Admin";
        public const string Empleado = "Empleado";

        [Key]
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Descripcion { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
