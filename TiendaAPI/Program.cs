using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TiendaAPI.Data;
using TiendaAPI.Models;
using TiendaAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Base de datos ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Servicios propios ----------
builder.Services.AddScoped<ITokenService, TokenService>();

// ---------- Controladores ----------
builder.Services.AddControllers();

// ---------- CORS permisivo (requerido por la consigna) ----------
// NOTA: "permisivo" es válido para un trabajo práctico académico.
// En un entorno productivo real, se recomienda restringir a los orígenes conocidos del cliente.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ---------- Autenticación JWT ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ---------- Swagger con soporte de JWT (botón "Authorize") ----------
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ---------- Migraciones automáticas + seed del usuario admin ----------
// Práctico para hosting gratuito, donde no siempre se tiene acceso a consola remota.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario
        {
            NombreUsuario = "admin",
            Email = "admin@tienda.com",
            Rol = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123$")
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // sirve wwwroot/uploads

app.UseCors("PermitirTodo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
