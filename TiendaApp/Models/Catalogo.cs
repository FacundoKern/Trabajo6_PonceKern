namespace TiendaApp.Models;

public class Producto
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

    public bool SinStock => Stock <= 0;
}

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// Misma forma que PagedResult<T> de la API
public class PaginaResultado<T>
{
    public List<T> Items { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TamanioPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public bool TienePaginaSiguiente { get; set; }
}
