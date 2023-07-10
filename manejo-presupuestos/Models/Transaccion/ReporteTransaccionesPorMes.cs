using manejo_presupuestos.Models.Categorias;

namespace manejo_presupuestos.Models.Transaccion
{
    public class ReporteTransaccionesPorMes
    {
        public int Mes { get; set; }

        public DateTime FechaReferencia { get; set; }

        public decimal Monto { get; set; }

        public decimal Ingreso { get; set; }

        public decimal Gasto { get; set; }

        public TipoOperacion TipoOperacionId { get; set; }
    }
}
