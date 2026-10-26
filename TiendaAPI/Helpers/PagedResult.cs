namespace TiendaAPI.Helpers
{
    // Envoltorio genérico para cualquier listado paginado
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas => TamanioPagina == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanioPagina);
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    // Parámetros de entrada estándar para cualquier endpoint paginado
    public class PaginacionParams
    {
        private const int TamanioMaximo = 50;
        private int _tamanioPagina = 10;

        public int Pagina { get; set; } = 1;

        public int TamanioPagina
        {
            get => _tamanioPagina;
            set => _tamanioPagina = value > TamanioMaximo ? TamanioMaximo : (value < 1 ? 1 : value);
        }

        // Búsqueda opcional por texto (nombre, razón social, etc.)
        public string? Buscar { get; set; }
    }
}
