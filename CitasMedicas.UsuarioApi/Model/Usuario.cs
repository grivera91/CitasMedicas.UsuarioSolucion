using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitasMedicas.UsuarioApi.Model
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }  // Clave primaria generada automáticamente

        [Required]
        [MaxLength(8)]
        public string CodigoUsuario { get; set; }  // Código único del usuario

        [Required]
        [MaxLength(20)]
        public string Nombre { get; set; }  // Nombre del usuario

        [Required]
        [MaxLength(20)]
        public string ApellidoPaterno { get; set; }  // Apellido paterno del usuario

        [MaxLength(20)]
        public string? ApellidoMaterno { get; set; }  // Apellido materno del usuario, nullable

        [Required]
        [Range(0, 99999999)]
        public int Dni { get; set; }  // Documento de identidad del usuario, debe ser único

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string CorreoElectronico { get; set; }  // Correo electrónico, debe ser único

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaNacimiento { get; set; }  // Fecha de nacimiento del usuario

        [Required]
        public int Genero { get; set; }  // Género del usuario (referencia a una tabla de géneros)

        [Required]
        public int NumeroTelefonico { get; set; }  // Número telefónico del usuario

        [Required]
        [MaxLength(100)]
        public string Direccion { get; set; }  // Dirección del usuario

        [MaxLength(20)]
        public string? UsuarioAcceso { get; set; }  // Nombre de usuario para roles administrativos, nullable para pacientes

        [Required]
        [MaxLength(255)]
        public string Contrasenia { get; set; }  // Contraseña del usuario (debe ser encriptada)

        [Column(TypeName = "date")]
        public DateTime? ContraseniaVencimiento { get; set; }  // Fecha de vencimiento de la contraseña

        public DateTime? UltimoAcceso { get; set; }  // Última fecha de acceso del usuario

        public int? RolUsuario { get; set; }  // Clave foránea que referencia al rol del usuario (nullable para pacientes)

        public bool EsActivo { get; set; } = true;  // Estado activo/inactivo del usuario (valor por defecto true)

        public bool EsAdmin { get; set; } = false;  // Indica si el usuario es administrador, por defecto es false

        [Required]
        [MaxLength(20)]
        public string UsuarioCreacion { get; set; } = "Sistema";  // Usuario que creó este registro, con valor por defecto

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;  // Fecha de creación del registro

        [MaxLength(20)]
        public string? UsuarioModificacion { get; set; }  // Usuario que modificó este registro

        public DateTime? FechaModificacion { get; set; }  // Fecha de la última modificación
    }
}
