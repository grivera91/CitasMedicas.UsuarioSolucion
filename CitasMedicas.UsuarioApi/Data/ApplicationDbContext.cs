using CitasMedicas.UsuarioApi.Model;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.UsuarioApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            base.OnModelCreating(modelBuilder);
        }
    }
}
