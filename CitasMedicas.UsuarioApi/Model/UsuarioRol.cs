﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.UsuarioApi.Model
{
    public class UsuarioRol
    {
        [Key]
        public int IdUsuarioRol { get; set; }
        public int IdUsuario { get; set; }
        public int IdRol { get; set; } 
        public bool RolActivo { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }
}