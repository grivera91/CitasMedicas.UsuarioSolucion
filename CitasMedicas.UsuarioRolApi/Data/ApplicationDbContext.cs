using Microsoft.EntityFrameworkCore;
using CitasMedicas.UsuarioRolApi.Model;

namespace CitasMedicas.UsuarioRolApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UsuarioRol> UsuarioRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioRol>().ToTable("UsuarioRol");
            base.OnModelCreating(modelBuilder);
        }
    }
}
