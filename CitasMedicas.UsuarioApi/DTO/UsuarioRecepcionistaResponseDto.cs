namespace CitasMedicas.UsuarioApi.DTO
{
    public class UsuarioRecepcionistaResponseDto
    {
        // Información del usuario
        public int IdUsuario { get; set; }
        public string CodigoUsuario { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int? Dni { get; set; }
        public string CorreoElectronico { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int Genero { get; set; }
        public int NumeroTelefonico { get; set; }
        public string Direccion { get; set; }

        // Información del médico (opcional)
        public RecepcionistaResponseDto? Recepcionista { get; set; }
    }
}
