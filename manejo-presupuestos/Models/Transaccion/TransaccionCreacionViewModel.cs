using manejo_presupuestos.Models.Categorias;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models.Transaccion
{
    public class TransaccionCreacionViewModel : Transaccion
    {
        public IEnumerable<SelectListItem>? Cuentas { get; set; }
        public IEnumerable<SelectListItem>? Categorias { get; set; }

        [Display(Name = "Tipo de operacion")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
    }
}
