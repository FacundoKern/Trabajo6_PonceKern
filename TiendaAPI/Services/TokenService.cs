using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TiendaAPI.Models;

namespace TiendaAPI.Services
{
    public interface ITokenService
    {
        (string Token, DateTime Expira) GenerarToken(Usuario usuario);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        // abstracción que representa un conjunto de propiedades de configuración de la aplicación
        // en formato clave-valor de solo lectura

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        // El usuario tiene que venir con el Rol cargado (Include(u => u.Rol))
        public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
        {
            var rol = usuario.Rol?.Nombre
                ?? throw new InvalidOperationException("El usuario no tiene el rol cargado");

            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection["Key"]
                ?? throw new InvalidOperationException("Falta configurar Jwt:Key en appsettings.json");
            var horasExpira = double.Parse(jwtSection["ExpiraHoras"] ?? "8");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, usuario.NombreUsuario),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(ClaimTypes.Role, rol),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var expira = DateTime.UtcNow.AddHours(horasExpira);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}
