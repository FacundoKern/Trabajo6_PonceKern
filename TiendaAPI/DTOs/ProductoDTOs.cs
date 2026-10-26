using System.ComponentModel.DataAnnotations;

namespace TiendaAPI.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
    }

    public class ProductoCreateDto
    {
        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [Required]
        public int CategoriaId { get; set; }
    }

    public class ProductoUpdateDto
    {
        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
