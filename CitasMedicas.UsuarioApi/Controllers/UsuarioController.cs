using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicas.UsuarioApi.Data;
using CitasMedicas.UsuarioApi.DTO;
using CitasMedicas.UsuarioApi.Model;
using Microsoft.AspNetCore.Authorization;
using CitasMedicas.UsuarioApi.Services;

namespace CitasMedicas.UserRegistrationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;        
        private readonly CorrelativoService _correlativoService;

        public UsuarioController(ApplicationDbContext context, CorrelativoService correlativoService)
        {
            _context = context;            
            _correlativoService = correlativoService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UsuarioCreateDto usuarioDto)
        {
            // Iniciar la transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // Validar que el nombre de usuario sea único solo si no es null o vacío
                if (!string.IsNullOrEmpty(usuarioDto.UsuarioAcceso) &&
                    await _context.Usuarios.AnyAsync(u => u.UsuarioAcceso == usuarioDto.UsuarioAcceso))
                {
                    return BadRequest("El nombre de usuario ya está en uso.");
                }

                // Obtener el correlativo para el código de usuario basado en el rol
                string codigoUsuario = await _correlativoService.ObtenerNuevoCorrelativoAsync(0);

                // Crear la entidad de usuario
                Usuario usuario = new Usuario
                {
                    CodigoUsuario = codigoUsuario,
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
                    EsActivo = true,
                    EsAdmin = usuarioDto.EsAdmin,
                    UsuarioCreacion = usuarioDto.UsuarioCreacion,
                    FechaCreacion = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();                   

                // Confirmar la transacción
                await transaction.CommitAsync();

                // Mapear la entidad `Usuario` a `UsuarioResponseDto`
                UsuarioResponseDto usuarioResponse = new UsuarioResponseDto
                {
                    IdUsuario = usuario.IdUsuario, // ID generado por la base de datos
                    CodigoUsuario = usuario.CodigoUsuario,
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
                    RolUsuario = usuarioDto.RolUsuario,    
                    EsAdmin = usuarioDto.EsAdmin,
                    EsActivo = usuario.EsActivo
                };
                
                return Ok(usuarioResponse);
            }
            catch (Exception ex)
            {
                // Deshacer la transacción si algo falla
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error en el registro: {ex.Message}");
            }            
        }

        //[Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditarUsuario(int id, UsuarioUpdateDto usuarioUpdateDto)
        {
            // Iniciar la transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // Obtener el rol activo actual del usuario
                //var rolActual = await _usuarioRolService.ObtenerRolActivoAsync(id);

                // Solo realizar el cambio si el rol actual es diferente al nuevo rol proporcionado
                //if (usuarioUpdateDto.IdRol.HasValue && usuarioUpdateDto.IdRol.Value != rolActual)
                //{
                //    var usuarioRolDto = new UsuarioRolDto
                //    {
                //        IdUsuario = id,
                //        IdRol = usuarioUpdateDto.IdRol.Value
                //    };

                //    var (success, message) = await _usuarioRolService.AsignarRolAsync(usuarioRolDto);
                //    if (!success)
                //    {
                //        return Conflict(new { message });
                //    }
                //}

                // Actualizar los datos de auditoría
                usuario.UsuarioModificacion = usuarioUpdateDto.UsuarioModificacion;
                usuario.FechaModificacion = DateTime.Now;

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();

                return Ok(new { message = "Usuario actualizado con éxito." });
            }
            catch (Exception ex)
            {
                // Deshacer la transacción si algo falla
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error en el registro: {ex.Message}");
            }            
        }

        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ListarUsuarios(
            [FromQuery] int? rol,
            [FromQuery] bool? esAdmin,
            [FromQuery] string? busqueda)
        {
            // Inicia la consulta base
            IQueryable<Usuario> query = _context.Usuarios;

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

            // Obtener los usuarios y los roles activos en una sola consulta
            var usuarios = await query.ToListAsync();
            var rolesActivos = await _context.UsuarioRoles
                .Where(ur => ur.RolActivo)
                .ToListAsync();

            // Crear una lista de respuesta
            var usuariosResponse = usuarios.Select(usuario =>
            {
                // Obtener el rol activo para el usuario actual
                int rolActivo = rolesActivos
                    .FirstOrDefault(ur => ur.IdUsuario == usuario.IdUsuario)?.IdRol ?? 0;

                return new UsuarioResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    ApellidoPaterno = usuario.ApellidoPaterno,
                    ApellidoMaterno = usuario.ApellidoMaterno,
                    Dni = usuario.Dni,
                    CorreoElectronico = usuario.CorreoElectronico,
                    FechaNacimiento = usuario.FechaNacimiento,
                    Genero = usuario.Genero,
                    NumeroTelefonico = usuario.NumeroTelefonico,
                    Direccion = usuario.Direccion,
                    RolUsuario = usuario.RolUsuario,
                    EsAdmin = usuario.EsAdmin,
                    EsActivo = usuario.EsActivo
                };
            }).ToList();

            return Ok(usuariosResponse);
        }

        //[Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerUsuario(int id)
        {
            UsuarioResponseDto? usuarioResponseDto = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
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

        //[Authorize]
        [HttpPatch("cambiar-estado/{id}")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id)
        {
            // Iniciar la transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Usuario? usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                // Alternar el estado de EsActivo
                usuario.EsActivo = !usuario.EsActivo;
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();

                string estado = usuario.EsActivo ? "activado" : "desactivado";
                return Ok(new { message = $"Usuario {estado} con éxito." });
            }
            catch (Exception ex)
            {
                // Deshacer la transacción si algo falla
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error en el registro: {ex.Message}");
            }           
        }

        //[Authorize]
        [HttpGet("dni/{dni}")]        
        public async Task<ActionResult<UsuarioResponseDto>> BuscarUsuarioDni(int dni)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Dni == dni)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,                    
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(); // Devolver 404 si el usuario no existe
            }

            return Ok(usuario); // Devolver datos del usuario si existe
        }

        //[Authorize]
        [HttpGet("rolUsuario/{rolUsuario}")]
        public async Task<ActionResult<UsuarioResponseDto>> BuscarUsuarioRol([FromRoute] int rolUsuario)
        {
            List<UsuarioResponseDto> usuarios = await _context.Usuarios
                .Where(u => u.RolUsuario == rolUsuario)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                })
                .ToListAsync();

            if (usuarios == null)
            {
                return NotFound(); // Devolver 404 si el usuario no existe
            }

            return Ok(usuarios); // Devolver datos del usuario si existe
        }

        //[Authorize]
        [HttpGet("usuarioPaciente")]
        public async Task<ActionResult<IEnumerable<UsuarioPacienteResponseDto>>> ObtenerUsuariosPacientes()
        {
            List<UsuarioPacienteResponseDto> usuariosPacientes = await _context.Usuarios
                .Where(u => u.RolUsuario == null)  // Filtra por usuarios con rol de médico
                .Select(u => new UsuarioPacienteResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                    Paciente = _context.Pacientes
                        .Where(p => p.IdUsuario == u.IdUsuario)
                        .Select(p => new PacienteCreateResponseDto
                        {
                            IdPaciente = p.IdPaciente,
                            IdUsuario = p.IdUsuario,
                            CodigoPaciente = p.CodigoPaciente,
                            CodigoHistoriaClinica = p.CodigoHistoriaClinica,
                            IdTipoSangre = p.IdTipoSangre,
                            Alergias = p.Alergias,                            
                            EnfermedadesPreexistentes = p.EnfermedadesPreexistentes,                            
                            ContactoEmergencia = p.ContactoEmergencia,                            
                            NumeroContactoEmergencia = p.NumeroContactoEmergencia,                            
                            Observaciones = p.Observaciones,                            
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(usuariosPacientes);
        }

        //[Authorize]
        [HttpGet("usuarioMedico")]
        public async Task<ActionResult<IEnumerable<UsuarioMedicoResponseDto>>> ObtenerUsuariosMedicos()
        {
            var usuariosConMedicos = await _context.Usuarios
                .Where(u => u.RolUsuario == 2)  // Filtra por usuarios con rol de médico
                .Select(u => new UsuarioMedicoResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                    Medico = _context.Medicos
                        .Where(m => m.IdUsuario == u.IdUsuario)
                        .Select(m => new MedicoResponseDto
                        {
                            IdMedico = m.IdMedico,
                            IdUsuario = m.IdUsuario,
                            CodigoMedico = m.CodigoMedico,
                            Especialidad = m.Especialidad,
                            NumeroColegiatura = m.NumeroColegiatura,
                            HorariosAtencion = m.HorariosAtencion.Select(h => new HorarioAtencionResponseDto
                            {
                                IdHorario = h.IdHorario,
                                IdMedico = h.IdMedico,
                                DiaSemana = h.DiaSemana,
                                HoraInicio = h.HoraInicio,
                                HoraFin = h.HoraFin
                            }).ToList()
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(usuariosConMedicos);
        }

        //[Authorize]
        [HttpGet("usuarioRecepcionista")]
        public async Task<ActionResult<IEnumerable<UsuarioMedicoResponseDto>>> ObtenerUsuarioRecepcionistas()
        {
            var usuariosRecepcionistas = await _context.Usuarios
                .Where(u => u.RolUsuario == 1)  // Filtra por usuarios con rol de recepcionista
                .Select(u => new UsuarioRecepcionistaResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoUsuario = u.CodigoUsuario,
                    Nombre = u.Nombre,
                    ApellidoPaterno = u.ApellidoPaterno,
                    ApellidoMaterno = u.ApellidoMaterno,
                    Dni = u.Dni,
                    CorreoElectronico = u.CorreoElectronico,
                    FechaNacimiento = u.FechaNacimiento,
                    Genero = u.Genero,
                    NumeroTelefonico = u.NumeroTelefonico,
                    Direccion = u.Direccion,
                    Recepcionista = _context.Recepcionistas
                        .Where(r => r.IdUsuario == u.IdUsuario)
                        .Select(r => new RecepcionistaResponseDto
                        {
                            IdRecepcionista = r.IdRecepcionista,
                            IdUsuario = r.IdUsuario,
                            CodigoRecepcionista = r.CodigoRecepcionista,
                            FechaContratacion = r.FechaContratacion,
                            Turno = r.Turno,
                            Departamento = r.Departamento,                            
                            EsActivo = r.EsActivo,
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(usuariosRecepcionistas);
        }
    }
}