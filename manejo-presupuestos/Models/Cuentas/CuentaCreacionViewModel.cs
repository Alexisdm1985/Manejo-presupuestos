using Microsoft.AspNetCore.Mvc.Rendering;

namespace manejo_presupuestos.Models.Cuenta
{
    public class CuentaCreacionViewModel : Cuentas
    {
        public IEnumerable<SelectListItem>? TiposCuentas { get; set; }
    }
}
