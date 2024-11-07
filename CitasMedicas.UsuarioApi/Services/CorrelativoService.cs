using CitasMedicas.UsuarioApi.Data;
using CitasMedicas.UsuarioApi.Model;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.UsuarioApi.Services
{
    public class CorrelativoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CorrelativoService> _logger;

        public CorrelativoService(ApplicationDbContext context, ILogger<CorrelativoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> ObtenerNuevoCorrelativoAsync(int rolUsuario)
        {
            try
            {
                string prefijoCorrelativo;

                // Determinar el prefijo según el rol
                switch (rolUsuario)
                {
                    case 1: // Recepcionista
                        prefijoCorrelativo = "CR";
                        break;
                    case 2: // Médico
                        prefijoCorrelativo = "CM";
                        break;
                    case 3: // Paciente
                        prefijoCorrelativo = "CP";
                        break;
                    case 9: // Administrador
                        prefijoCorrelativo = "CA";
                        break;
                    default: // Código de usuario genérico
                        prefijoCorrelativo = "CU";
                        break;
                }


                // Buscar el correlativo según el prefijo
                Correlativo? correlativo = await _context.Correlativos
                    .FirstOrDefaultAsync(c => c.Prefijo == prefijoCorrelativo);

                if (correlativo == null)
                {
                    throw new Exception("No se encontró un correlativo con el prefijo especificado.");
                }

                // Incrementar el último número y actualizar la fecha de actualización
                correlativo.UltimoNumero += 1;
                correlativo.FechaActulizacion = DateTime.Now;

                // Generar el nuevo código con el prefijo y el número rellenado con ceros
                string numeroRelleno = correlativo.UltimoNumero.ToString().PadLeft(6, '0');
                string nuevoCodigo = $"{correlativo.Prefijo}{numeroRelleno}";

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return nuevoCodigo;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al obtener el correlativo: {0}", ex.Message);
                throw;
            }
        }
    }
}
