using System.ComponentModel.DataAnnotations;

namespace CeplanAuth.Models
{

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = "DNI"; 

        public string? MensajeError { get; set; }
        public string? TipoError { get; set; } 
        public int CVF { get; set; } = 0;
    }

    public class UsuarioModel
    {

        public int Id { get; set; }
        public string TipoDocumento { get; set; } = "DNI";
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string? Cargo { get; set; }
        public string? Dependencia { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? CorreoPrincipal { get; set; }
        public string? CorreoSecundario { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? TelefonoSecundario { get; set; }
        public string? Nacionalidad { get; set; }
        public string? TipoContratacion { get; set; }
        public DateTime? FechaContratacion { get; set; }
        public string Estado { get; set; } = "Activo";
        public int CVF { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }

        public string NombreCompleto => $"{PrimerApellido} {SegundoApellido}, {Nombres}".Trim();
        public string FechaNacimientoFormatted => FechaNacimiento?.ToString("dd/MM/yyyy") ?? "";
        public string FechaContratacionFormatted => FechaContratacion?.ToString("dd/MM/yyyy") ?? "";
    }


    public class ResultadoLogin
    {
        public string Resultado { get; set; } = string.Empty; 
        public string Mensaje { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public int CVF { get; set; }
    }
}
