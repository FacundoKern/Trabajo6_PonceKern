namespace TiendaAPI.DTOs
{
    public class AuditoriaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public int? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? Ip { get; set; }
    }

    public class LogErrorDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? TipoExcepcion { get; set; }
        public string? Metodo { get; set; }
        public string? Ruta { get; set; }
        public string? NombreUsuario { get; set; }

        // Solo se completa al pedir un error puntual (GET api/auditoria/errores/5)
        public string? StackTrace { get; set; }
    }
}
