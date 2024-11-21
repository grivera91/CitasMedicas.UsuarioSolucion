using CitasMedicas.UsuarioApi.Model;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.UsuarioApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }
        public DbSet<Correlativo> Correlativos { get; set; }
        public DbSet<Medico> Medicos { get; set; }        
        public DbSet<Recepcionista> Recepcionistas { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<UsuarioRol>().ToTable("UsuarioRol");
            modelBuilder.Entity<Correlativo>().ToTable("Correlativo");
            modelBuilder.Entity<Medico>().ToTable("Medico");            
            modelBuilder.Entity<Recepcionista>().ToTable("Recepcionista");
            modelBuilder.Entity<Paciente>().ToTable("Paciente");
            base.OnModelCreating(modelBuilder);
        }
    }
}
