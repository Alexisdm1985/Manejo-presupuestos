using manejo_presupuestos.Models.Transaccion;

namespace manejo_presupuestos.Models
{
    public class ReporteMensualViewModel
    {
        public IEnumerable<ReporteTransaccionesPorMes> TransaccionesPorMes { get; set; }

        public decimal Ingresos => TransaccionesPorMes.Sum(x => x.Ingreso);

        public decimal Gastos => TransaccionesPorMes.Sum(x => x.Gasto);

        public decimal Total => Ingresos - Gastos;

        public int Anio { get; set; }
    }
}
