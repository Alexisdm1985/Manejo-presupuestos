using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo valido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Contrasenia { get; set; }

        public bool Recuerdame { get; set; }
    }
}
