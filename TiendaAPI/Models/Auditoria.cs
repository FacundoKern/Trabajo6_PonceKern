using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.Models
{
    // Registro de auditoría: quién hizo qué, sobre qué y cuándo.
    // Ej: Accion = "Crear", Entidad = "Producto", EntidadId = 5
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Null cuando no hay usuario identificado (ej: login fallido)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required, MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Entidad { get; set; } = string.Empty;

        public int? EntidadId { get; set; }

        [MaxLength(500)]
        public string? Detalle { get; set; }

        [MaxLength(45)]
        public string? Ip { get; set; }
    }
}
