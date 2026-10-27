namespace TiendaApp.Models;

// Lo que se manda a POST api/auth/login
public class LoginRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Lo que devuelve la API al loguearse; se guarda tal cual como sesión del usuario
public class Sesion
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    public bool Vencida => Expira.ToUniversalTime() <= DateTime.UtcNow;
}
