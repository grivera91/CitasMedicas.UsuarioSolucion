﻿namespace CitasMedicas.UsuarioApi.DTO
{
    public class PacienteCreateRequestDto
    {       
        public int IdUsuario { get; set; } 
        public string NumeroHistoriaClinica { get; set; }
        public int? IdTipoSangre { get; set; }
        public string Alergias { get; set; }
        public string EnfermedadesPreexistentes { get; set; }        
        public string ContactoEmergencia { get; set; }        
        public string NumeroContactoEmergencia { get; set; }
        public string Observaciones { get; set; }        
        public string UsuarioCreacion { get; set; }
    }
}
