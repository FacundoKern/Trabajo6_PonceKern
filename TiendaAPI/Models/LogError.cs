using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.Models
{
    // Errores no controlados de la API (los guarda ManejoErroresMiddleware)
    public class LogError
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        // Tipo de la excepción, ej: "System.NullReferenceException"
        [MaxLength(200)]
        public string? TipoExcepcion { get; set; }

        // Detalle técnico completo (sin límite de largo)
        public string? StackTrace { get; set; }

        // Request que falló, ej: "POST /api/salidas"
        [MaxLength(10)]
        public string? Metodo { get; set; }

        [MaxLength(300)]
        public string? Ruta { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
