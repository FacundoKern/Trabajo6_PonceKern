using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Data;
using TiendaAPI.DTOs;
using TiendaAPI.Helpers;
using TiendaAPI.Models;
using TiendaAPI.Services;

namespace TiendaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public ClientesController(ApplicationDbContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private static ClienteDto ToDto(Cliente c) => new()
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Dni = c.Dni,
            Email = c.Email,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            Activo = c.Activo
        };

        // GET: api/clientes?pagina=1&tamanioPagina=10&buscar=perez
        [HttpGet]
        public async Task<ActionResult<PagedResult<ClienteDto>>> GetClientes([FromQuery] PaginacionParams parametros)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
                query = query.Where(c => c.Nombre.Contains(parametros.Buscar) || c.Apellido.Contains(parametros.Buscar));

            var resultado = await query
                .OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
                .Select(c => ToDto(c))
                .ToPagedResultAsync(parametros.Pagina, parametros.TamanioPagina);

            return Ok(resultado);
        }

        // GET: api/clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { mensaje = "Cliente no encontrado" });

            return Ok(ToDto(cliente));
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CrearCliente(ClienteCreateDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("Crear", "Cliente", cliente.Id, $"{cliente.Nombre} {cliente.Apellido}");

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, ToDto(cliente));
        }

        // PUT: api/clientes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarCliente(int id, ClienteCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { mensaje = "Cliente no encontrado" });

            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Dni = dto.Dni;
            cliente.Email = dto.Email;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("Modificar", "Cliente", cliente.Id, $"{cliente.Nombre} {cliente.Apellido}");
            return NoContent();
        }

        // DELETE: api/clientes/5 (baja lógica para no romper historial de ventas)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { mensaje = "Cliente no encontrado" });

            cliente.Activo = false;
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("Baja", "Cliente", cliente.Id, $"{cliente.Nombre} {cliente.Apellido}");
            return NoContent();
        }
    }
}
