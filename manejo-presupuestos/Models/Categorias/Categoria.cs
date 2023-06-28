using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models.Categorias
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="El campo {0} es requerido")]
        [StringLength(maximumLength:50, ErrorMessage = "No puede ser mayor a {1} caracteres")]
        public string Nombre { get; set; }

        [Display(Name ="Tipo de categoria")]
        public TipoOperacion TipoOperacionesId { get; set; }

        public int UsuarioId { get; set; }
    }
}
