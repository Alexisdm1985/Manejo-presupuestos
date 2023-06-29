using manejo_presupuestos.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace manejo_presupuestos.Models.Transaccion
{
    public class TransaccionActualizarViewModel : TransaccionCreacionViewModel
    {
        public decimal MontoAnterior { get; set; }
        public int CuentaAnteriorId { get; set; }
    }
}
