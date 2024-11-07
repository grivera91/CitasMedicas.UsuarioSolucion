using CitasMedicas.UsuarioRolApi.Data;
using CitasMedicas.UsuarioRolApi.DTO;
using CitasMedicas.UsuarioRolApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.UsuarioRolApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioRolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRolController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AsignarRol([FromBody] UsuarioRolDto usuarioRolDto)
        {
            // Verificar si el usuario ya tiene el rol asignado
            var existeRol = await _context.UsuarioRoles
                .AnyAsync(ur => ur.IdUsuario == usuarioRolDto.IdUsuario && ur.IdRol == usuarioRolDto.IdRol);

            if (existeRol)
            {
                return Conflict("El usuario ya tiene asignado este rol.");
            }

            UsuarioRol usuarioRol = new UsuarioRol
            {
                IdUsuario = usuarioRolDto.IdUsuario,
                IdRol = usuarioRolDto.IdRol,
                FechaAsignacion = DateTime.Now
            };

            // Asignar rol al usuario            
            _context.UsuarioRoles.Add(usuarioRol);
            await _context.SaveChangesAsync();

            return Ok("Rol asignado exitosamente.");
        }
    }
}