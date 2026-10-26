using Microsoft.EntityFrameworkCore;

namespace TiendaAPI.Helpers
{
    public static class QueryableExtensions
    {
        // Aplica Skip/Take y arma el PagedResult a partir de un IQueryable ya filtrado/ordenado
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int pagina, int tamanioPagina)
        {
            var total = await query.CountAsync();

            var items = await query
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PaginaActual = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = total
            };
        }
    }
}
