using System.ComponentModel.DataAnnotations;

namespace TiendaApp.Models;

// UsuarioDto de la API
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string? RolNombre { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

// POST api/auth/registro (el rol va por nombre)
public class NuevoUsuario
{
    [Required(ErrorMessage = "Ingresá el nombre de usuario.")]
    [MaxLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el email.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá la contraseña.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = "Empleado";
}

// PUT api/usuarios/{id} (el rol va por id)
public class UsuarioEditar
{
    [Required(ErrorMessage = "Ingresá el email.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
}

// PUT api/usuarios/{id}/password
public class CambiarPassword
{
    [Required(ErrorMessage = "Ingresá la contraseña nueva.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadUsuarios { get; set; }

    // La API no deja renombrar ni borrar los roles que usan los [Authorize]
    public bool EsBase => Nombre is "Admin" or "Empleado";
}

// POST y PUT api/roles
public class RolRequest
{
    [Required(ErrorMessage = "Ingresá el nombre del rol.")]
    [MaxLength(30, ErrorMessage = "Máximo 30 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
    public string? Descripcion { get; set; }
}

// AuditoriaDto de la API
public class RegistroAuditoria
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

// LogErrorDto de la API. StackTrace solo viene al pedir un error puntual
public class LogError
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? TipoExcepcion { get; set; }
    public string? Metodo { get; set; }
    public string? Ruta { get; set; }
    public string? NombreUsuario { get; set; }
    public string? StackTrace { get; set; }
}
