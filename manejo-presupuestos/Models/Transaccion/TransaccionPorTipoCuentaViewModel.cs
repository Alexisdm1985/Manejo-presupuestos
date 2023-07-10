namespace manejo_presupuestos.Models.Transaccion
{
    public class TransaccionPorTipoCuentaViewModel
    {
        public int UsuarioId { get; set; }
        public int CuentaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

    }
}
