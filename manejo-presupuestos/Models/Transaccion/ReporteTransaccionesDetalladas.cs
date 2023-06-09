﻿using manejo_presupuestos.Models.Categorias;

namespace manejo_presupuestos.Models.Transaccion
{
    public class ReporteTransaccionesDetalladas
    {
        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public IEnumerable<TransaccionesPorFecha> TransaccionesAgrupadas { get; set; }

        public decimal BalanceRetiros => TransaccionesAgrupadas.Sum(x => x.BalanceRetiros);

        public decimal BalanceDepositos => TransaccionesAgrupadas.Sum(x => x.BalanceDepositos);

        public decimal Total => BalanceDepositos - BalanceRetiros;

        public class TransaccionesPorFecha
        {
            public DateTime FechaTransaccion { get; set; }

            public IEnumerable<Transaccion> Transacciones { get; set;}


            // Obtiene todos los ingresos y los suma
            public decimal BalanceDepositos => 
                Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                .Sum(x => x.Monto);

            public decimal BalanceRetiros =>
                Transacciones.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                .Sum(x => x.Monto);
        }
    }
}
