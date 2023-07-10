using manejo_presupuestos.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models.Transaccion
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Display(Name ="Fecha transaccion")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;

        public decimal Monto { get; set; }
        public int TipoTransaccionId { get; set; }

        [StringLength(maximumLength: 10000, ErrorMessage = "La nota no puede pasar de {1} caracteres")]
        public string? Nota { get; set; }
        
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Cuenta")]
        [Display(Name ="Cuenta")]
        public int CuentaId { get; set; }

        [Display(Name = "Categoria")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Categoria")]
        public int CategoriaId { get; set; }

        [Display(Name = "Tipo de operacion")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;

        public string? Cuenta { get; set; }
        public string? Categoria{ get; set; }

    }
}
