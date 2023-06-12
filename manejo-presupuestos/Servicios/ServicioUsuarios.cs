namespace manejo_presupuestos.Servicios
{
    public interface IServicioUsuarios
    {
        public int ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {

        public int ObtenerUsuarioId()
        {
            return 1;
        }
    }
}
