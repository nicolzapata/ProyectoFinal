using GestionUsuarios.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<AuditoriaUsuario> AuditoriaUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Permiso>()
                .HasIndex(p => p.CodigoPermiso)
                .IsUnique();

            builder.Entity<RolPermiso>()
                .HasIndex(rp => new { rp.RolId, rp.PermisoId })
                .IsUnique();
        }
    }
}