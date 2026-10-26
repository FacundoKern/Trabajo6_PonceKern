using Microsoft.EntityFrameworkCore;
using TiendaAPI.Models;

namespace TiendaAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Ingreso> Ingresos => Set<Ingreso>();
        public DbSet<IngresoDetalle> IngresoDetalles => Set<IngresoDetalle>();
        public DbSet<Salida> Salidas => Set<Salida>();
        public DbSet<SalidaDetalle> SalidaDetalles => Set<SalidaDetalle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            // Producto -> Categoria (N:1)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // no borrar categoría si tiene productos

            // Ingreso -> Proveedor (N:1)
            modelBuilder.Entity<Ingreso>()
                .HasOne(i => i.Proveedor)
                .WithMany(pr => pr.Ingresos)
                .HasForeignKey(i => i.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // IngresoDetalle -> Ingreso (N:1, cascade al borrar cabecera)
            // Osea, si se borra un Ingreso, se borran sus IngresoDetalles asociados.
            modelBuilder.Entity<IngresoDetalle>()
                .HasOne(d => d.Ingreso)
                .WithMany(i => i.Detalles)
                .HasForeignKey(d => d.IngresoId)
                .OnDelete(DeleteBehavior.Cascade);

            // IngresoDetalle -> Producto (N:1)
            modelBuilder.Entity<IngresoDetalle>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.IngresoDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Salida -> Cliente (N:1)
            modelBuilder.Entity<Salida>()
                .HasOne(s => s.Cliente)
                .WithMany(c => c.Salidas)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // SalidaDetalle -> Salida (N:1, cascade al borrar cabecera)
            // Misma lógica que los ingresos: si se borra una Salida, se borran sus SalidaDetalles asociados.
            modelBuilder.Entity<SalidaDetalle>()
                .HasOne(d => d.Salida)
                .WithMany(s => s.Detalles)
                .HasForeignKey(d => d.SalidaId)
                .OnDelete(DeleteBehavior.Cascade);

            // SalidaDetalle -> Producto (N:1)
            modelBuilder.Entity<SalidaDetalle>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.SalidaDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // El usuario administrador inicial se crea en Program.cs al arrancar, no se inserta como dato semilla (HasData)
            // porque BCrypt necesita generar un hash con salt y parámetros en runtime; si se pone un hash fijo en código
            // o en HasData ese valor puede ser incorrecto, inseguro o quedar obsoleto
        }
    }
}
