using manejo_presupuestos.Models.Cuenta;
using manejo_presupuestos.Models.Transaccion;
using System.Reflection.Metadata;

namespace manejo_presupuestos.Servicios
{
    public interface IServicioReportes
    {
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anio, dynamic ViewBag);
        Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int anio, dynamic ViewBag);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteTransaccionesSemanal(int usuarioId, int mes, int anio, dynamic ViewBag);
    }

    public class ServicioReportes : IServicioReportes
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReportes(
            IRepositorioTransacciones repositorioTransacciones,
            IHttpContextAccessor httpContextAccessor)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anio, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            };

            var transacciones = await repositorioTransacciones.ObtenerTransaccionPorUsuarioId(parametro);

            var modelo = GenerarReporteTransaccionesDetallas(transacciones, fechaInicio, fechaFin);

            AsignarValoresViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteTransaccionesSemanal(int usuarioId, int mes, int anio, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            };

            var modelo= await repositorioTransacciones.ObtenerDetallePorSemana(parametro);

            AsignarValoresViewBag(ViewBag, fechaInicio);

            return modelo;

        }

        public async Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(
            int usuarioId, int cuentaId, int mes, int anio, dynamic ViewBag) 
        {

            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioYFin(mes, anio);

            var transaccionesPorCuenta = new TransaccionPorTipoCuentaViewModel()
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CuentaId = cuentaId,
                UsuarioId = usuarioId
            };
            
            var transacciones = await repositorioTransacciones.ObtenerTransaccionPorCuenta(transaccionesPorCuenta);

            var modelo = GenerarReporteTransaccionesDetallas(transacciones, fechaInicio, fechaFin);

            AsignarValoresViewBag(ViewBag, fechaInicio);
            return modelo;
        }

        public void AsignarValoresViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            // Para los botones de fecha anterior y siguiente en la vista
            //https://www.udemy.com/course/aprende-aspnet-core-mvc-haciendo-proyectos-desde-cero/learn/lecture/29581012#questions
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.anioAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesSiguiente = fechaInicio.AddMonths(1).Month;
            ViewBag.anioSiguiente = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private ReporteTransaccionesDetalladas GenerarReporteTransaccionesDetallas( IEnumerable<Transaccion> transacciones, DateTime fechaInicio, DateTime fechaFin)
        {

            // Crea el modelo para la vista de reportes de transacciones por cuenta
            var modelo = new ReporteTransaccionesDetalladas();

            // Agrupa las transacciones por fecha
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioYFin(int mes, int anio)
        {
            // Setea las fechas de inicio y fin segun validacion
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || anio <= 1900)
            {
                var fechaHoy = DateTime.Today;

                // Se obtiene anio y mes actual y el primer dia del mes.
                fechaInicio = new DateTime(fechaHoy.Year, fechaHoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(anio, mes, 1);
            };

            //Fecha fin siempre sera el ultimo dia del mes siguiente a la fecha de inicio
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }
    }

    
}
