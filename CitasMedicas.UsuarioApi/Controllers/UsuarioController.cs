using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicas.UsuarioApi.Data;
using CitasMedicas.UsuarioApi.DTO;
using CitasMedicas.UsuarioApi.Model;

namespace CitasMedicas.UserRegistrationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Register([FromBody] UsuarioCreateDto usuarioDto)
        {
            // Validar que el DNI sea único
            if (await _context.Usuarios.AnyAsync(u => u.Dni == usuarioDto.Dni))
            {
                return BadRequest("El DNI ya está registrado.");
            }

            // Validar que el correo electrónico sea único
            if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == usuarioDto.CorreoElectronico))
            {
                return BadRequest("El correo electrónico ya está en uso.");
            }

            // Validar que el nombre de usuario sea único
            if (await _context.Usuarios.AnyAsync(u => u.UsuarioAcceso == usuarioDto.UsuarioAcceso))
            {
                return BadRequest("El nombre de usuario ya está en uso.");
            }

            // Crear la entidad de usuario
            Usuario usuario = new Usuario
            {
                Nombre = usuarioDto.Nombre,
                ApellidoPaterno = usuarioDto.ApellidoPaterno,
                ApellidoMaterno = usuarioDto.ApellidoMaterno,
                Dni = usuarioDto.Dni,
                CorreoElectronico = usuarioDto.CorreoElectronico,
                FechaNacimiento = usuarioDto.FechaNacimiento,
                Genero = usuarioDto.Genero,
                NumeroTelefonico = usuarioDto.NumeroTelefonico,
                Direccion = usuarioDto.Direccion,
                UsuarioAcceso = usuarioDto.UsuarioAcceso,
                Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contrasenia),
                ContraseniaVencimiento = DateTime.Today.AddDays(90),
                RolUsuario = usuarioDto.RolUsuario,
                EsAdmin = usuarioDto.EsAdmin,
                EsActivo = true,
                UsuarioCreacion = usuarioDto.UsuarioCreacion,
                FechaCreacion = DateTime.Now                
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Mapear la entidad `Usuario` a `UsuarioResponseDto`
            UsuarioResponseDto usuarioResponse = new UsuarioResponseDto
            {
                IdUsuario = usuario.IdUsuario, // ID generado por la base de datos
                Nombre = usuario.Nombre,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Dni = usuario.Dni,
                CorreoElectronico = usuario.CorreoElectronico,
                FechaNacimiento = usuario.FechaNacimiento,
                Genero = usuario.Genero,
                NumeroTelefonico = usuario.NumeroTelefonico,
                Direccion = usuario.Direccion,
                UsuarioAcceso = usuario.UsuarioAcceso,
                RolUsuario = usuario.RolUsuario,
                EsAdmin = usuario.EsAdmin,
                EsActivo = usuario.EsActivo
            };

            //return Ok(new { message = "Usuario registrado con éxito" });
            return Ok(usuarioResponse);
        }

        [HttpPatch("editar/{id}")]
        public async Task<IActionResult> EditarUsuario(int id, UsuarioUpdateDto usuarioUpdateDto)
        {
            // Buscar el usuario en la base de datos
            Usuario? usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Validar que los campos únicos (DNI, correo, usuario) no estén en uso por otro usuario
            bool dniEnUso = usuarioUpdateDto.Dni != null && usuario.Dni != usuarioUpdateDto.Dni &&
                            await _context.Usuarios.AnyAsync(u => u.Dni == usuarioUpdateDto.Dni && u.IdUsuario != id);

            bool correoEnUso = !string.IsNullOrEmpty(usuarioUpdateDto.CorreoElectronico) && usuario.CorreoElectronico != usuarioUpdateDto.CorreoElectronico &&
                               await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == usuarioUpdateDto.CorreoElectronico && u.IdUsuario != id);

            if (dniEnUso)
            {
                return Conflict(new { message = "El DNI ya está registrado." });  // 409 Conflict
            }

            if (correoEnUso)
            {
                return Conflict(new { message = "El correo electrónico ya está en uso." });  // 409 Conflict
            }

            // Actualizar solo los campos proporcionados
            usuario.Nombre = usuarioUpdateDto.Nombre ?? usuario.Nombre;
            usuario.ApellidoPaterno = usuarioUpdateDto.ApellidoPaterno ?? usuario.ApellidoPaterno;
            usuario.ApellidoMaterno = usuarioUpdateDto.ApellidoMaterno ?? usuario.ApellidoMaterno;
            usuario.FechaNacimiento = usuarioUpdateDto.FechaNacimiento ?? usuario.FechaNacimiento;
            usuario.Genero = usuarioUpdateDto.Genero ?? usuario.Genero;
            usuario.NumeroTelefonico = usuarioUpdateDto.NumeroTelefonico ?? usuario.NumeroTelefonico;
            usuario.Direccion = usuarioUpdateDto.Direccion ?? usuario.Direccion;
            usuario.RolUsuario = usuarioUpdateDto.RolUsuario ?? usuario.RolUsuario;
            usuario.EsAdmin = usuarioUpdateDto.EsAdmin ?? usuario.EsAdmin;

            // Actualizar la contraseña solo si se proporciona
            if (!string.IsNullOrEmpty(usuarioUpdateDto.Contrasenia))
            {
                usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuarioUpdateDto.Contrasenia);
                usuario.ContraseniaVencimiento = DateTime.Today;
            }

            // Actualizar los datos de auditoría
            usuario.UsuarioModificacion = usuarioUpdateDto.UsuarioModificacion;
            usuario.FechaModificacion = DateTime.Now;

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario actualizado con éxito." });
        }

        [HttpGet("listar")]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ListarUsuarios(
            [FromQuery] int? rol,
            [FromQuery] bool? esAdmin,
            [FromQuery] string? busqueda)
        {
            // Inicia la consulta base
            IQueryable<Usuario> query = _context.Usuarios;

            // Filtro por rol si está presente
            if (rol.HasValue)
            {
                query = query.Where(u => u.RolUsuario == rol.Value);
            }

            // Filtro por si es administrador si está presente
            if (esAdmin.HasValue)
            {
                query = query.Where(u => u.EsAdmin == esAdmin.Value);
            }

            // Filtro por búsqueda en nombre, apellido paterno y apellido materno si está presente
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(u => u.Nombre.Contains(busqueda) ||
                                         u.ApellidoPaterno.Contains(busqueda) ||
                                         u.ApellidoMaterno.Contains(busqueda));
            }

            // Ejecuta la consulta
            List<UsuarioResponseDto> usuarios = await query
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                    RolUsuario = u.RolUsuario,
                    EsAdmin = u.EsAdmin,
                    EsActivo = u.EsActivo
                }).ToListAsync();

            return Ok(usuarios);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerUsuario(int id)
        {
            UsuarioResponseDto? usuarioResponseDto = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                    RolUsuario = u.RolUsuario,
                    EsAdmin = u.EsAdmin,
                    EsActivo = u.EsActivo
                })
                .FirstOrDefaultAsync();

            if (usuarioResponseDto == null)
            {
                return NotFound("Usuario no encontrado");
            }

            return Ok(usuarioResponseDto);
        }

        [HttpPatch("cambiar-estado/{id}")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id)
        {
            Usuario? usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Alternar el estado de EsActivo
            usuario.EsActivo = !usuario.EsActivo;
            await _context.SaveChangesAsync();

            string estado = usuario.EsActivo ? "activado" : "desactivado";
            return Ok(new { message = $"Usuario {estado} con éxito." });
        }
    }
}