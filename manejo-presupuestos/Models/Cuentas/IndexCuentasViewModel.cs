namespace manejo_presupuestos.Models.Cuenta
{
    public class IndexCuentasViewModel
    {
        public string TipoCuenta { get; set; }
        public IEnumerable<Cuentas> Cuentas { get; set; }
        public decimal Balance => Cuentas.Sum(c => c.Balance);
    }
}
