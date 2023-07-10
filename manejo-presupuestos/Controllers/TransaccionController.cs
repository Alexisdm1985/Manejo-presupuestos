using manejo_presupuestos.Models;
using manejo_presupuestos.Models.Categorias;
using manejo_presupuestos.Models.Transaccion;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace manejo_presupuestos.Controllers
{
    public class TransaccionController : Controller
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioReportes servicioReportes;

        public TransaccionController(
            IRepositorioTransacciones repositorioTransacciones,
            IServicioUsuarios servicioUsuarios,
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategorias repositorioCategorias,
            IServicioReportes servicioReportes)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.servicioReportes = servicioReportes;
        }




        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();

            // Obtengo las cuentas y categorias para el <select> en la vista
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel transaccion)
        {

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(transaccion.CuentaId, usuarioId);

            if (cuenta is null) RedirectToAction("NoEncontrado", "Home");

            var categoria = await repositorioCategorias.ObtenerCategoria(transaccion.CategoriaId, usuarioId);

            if (categoria is null) RedirectToAction("NoEncontrado", "Home");

            // Validaciones
            if (!ModelState.IsValid)
            {
                transaccion.Cuentas = await ObtenerCuentas(usuarioId);
                transaccion.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);

                return View(transaccion);
            }

            if (transaccion.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            transaccion.UsuarioId = usuarioId;

            await repositorioTransacciones.CrearTransaccion(transaccion);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.BuscarTransaccionPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = new TransaccionActualizarViewModel()
            {
                Nota = transaccion.Nota,
                TipoTransaccionId = transaccion.TipoTransaccionId,
                TipoOperacionId = transaccion.TipoOperacionId,
                CuentaId = transaccion.CuentaId,
                CuentaAnteriorId = transaccion.CuentaId,
                CategoriaId = transaccion.CategoriaId,
                Monto = transaccion.Monto,
                MontoAnterior = transaccion.Monto,
                UsuarioId = usuarioId,
                Id = id,
                FechaTransaccion = transaccion.FechaTransaccion,
                urlRetorno = urlRetorno
            };

            // Para dejar como negativo el valor si es gasto
            if (modelo.TipoOperacionId == TipoOperacion.Gasto) modelo.Monto *= -1;

            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);


            return View(modelo);


        }

        [HttpPost]
        public async Task<IActionResult> EditarTransaccion(TransaccionActualizarViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            // Validaciones
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null) RedirectToAction("NoEncontrado", "Home");

            var categoria = await repositorioCategorias.ObtenerCategoria(modelo.CategoriaId, usuarioId);

            if (categoria is null) RedirectToAction("NoEncontrado", "Home");

            // Realiza la actualizacion
            var transaccion = new TransaccionActualizarViewModel()
            {
                Nota = modelo.Nota,
                TipoOperacionId = modelo.TipoOperacionId,
                TipoTransaccionId = modelo.TipoTransaccionId,
                Monto = modelo.Monto,
                MontoAnterior = modelo.MontoAnterior,
                CuentaId = modelo.CuentaId,
                CategoriaId = modelo.CategoriaId,
                CuentaAnteriorId = modelo.CuentaAnteriorId,
                Id = modelo.Id,
                FechaTransaccion = modelo.FechaTransaccion
            };

            if (modelo.TipoOperacionId == TipoOperacion.Gasto) transaccion.Monto *= -1;

            await repositorioTransacciones.ActualizarTransaccion(transaccion.MontoAnterior, transaccion.CuentaAnteriorId, transaccion);

            if (string.IsNullOrEmpty(modelo.urlRetorno))
            {
                return RedirectToAction("Index");
            }

            return LocalRedirect(modelo.urlRetorno);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transaccion = await repositorioTransacciones.BuscarTransaccionPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }

            // Elimina la transaccion
            await repositorioTransacciones.EliminarTransaccion(id);

            return RedirectToAction("Index");
        }

        public async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.BuscarCuentas(usuarioId);

            if (cuentas is null) RedirectToAction("NoEncontrado", "Home");

            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.ObtenerPorTipoOperacion(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        //Peticion desde JS en la vista
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }


        // REPORTES
        public async Task<IActionResult> Index(int mes, int anio)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, anio, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int anio)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            IEnumerable<ResultadoObtenerPorSemana> transaccionesSemanales = await servicioReportes.ObtenerReporteTransaccionesSemanal(usuarioId, mes, anio, ViewBag);

            var agrupado = transaccionesSemanales.GroupBy(x => x.Semana)
                .Select(x => new ResultadoObtenerPorSemana()
                {
                    Semana = x.Key,
                    Ingresos = x.Where(x => x.TipoOperacionesId == TipoOperacion.Ingreso)
                        .Select(x => x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionesId == TipoOperacion.Gasto)
                    .Select(x => x.Monto).FirstOrDefault(),
                }).ToList();

            // Aquí se utiliza la clase Enumerable para generar una secuencia de números enteros.El método Range de la clase Enumerable toma dos argumentos: el valor inicial y la cantidad de elementos a generar. En este caso, el valor inicial es 1 y la cantidad de elementos es el número de días del mes.

            //Para determinar el número de días del mes, se realiza lo siguiente:

            // fechaReferencia.AddMonths(1) agrega un mes a la fecha de referencia, lo que nos lleva al primer día del mes siguiente.
            // AddDays(-1) resta un día a esa fecha, lo que nos lleva al último día del mes actual.
            // Day obtiene el número de día del último día del mes actual.

            if (anio == 0 || mes == 0)
            {

                anio = DateTime.Today.Year;
                mes = DateTime.Today.Month;
            }

            var fechaReferencia = new DateTime(anio, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);


            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anio, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(anio, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaFin = fechaFin,
                        FechaInicio = fechaInicio
                    });
                }else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel()
            {
                FechaReferencia = fechaReferencia,
                TransaccionesPorSemana = agrupado
            };

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int anio)
        {

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            if (anio == 0)
            {
                anio = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerDetallePorMes(usuarioId, anio);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                    .Select(x => new ReporteTransaccionesPorMes()
                    {
                        Mes = x.Key,
                        Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                            .Select(x => x.Monto).FirstOrDefault(),
                        Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                            .Select(x => x.Monto).FirstOrDefault()
                    }).ToList();

            for (int mes = 1; mes <= 12; mes ++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(anio, mes, 1);
                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ReporteTransaccionesPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();

            modelo.Anio = anio;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        public IActionResult Calendario()
        {
            return View();
        }
    }
}
