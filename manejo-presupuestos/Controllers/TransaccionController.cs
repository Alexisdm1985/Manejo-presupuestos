using manejo_presupuestos.Models.Categorias;
using manejo_presupuestos.Models.Transaccion;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;

namespace manejo_presupuestos.Controllers
{
    public class TransaccionController : Controller
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;

        public TransaccionController(
            IRepositorioTransacciones repositorioTransacciones,
            IServicioUsuarios servicioUsuarios,
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategorias repositorioCategorias)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
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
                transaccion.Monto *=  -1;
            }

            transaccion.UsuarioId = usuarioId;

            await repositorioTransacciones.CrearTransaccion(transaccion);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
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
                FechaTransaccion = transaccion.FechaTransaccion
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

            return RedirectToAction("Index");
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
    }
}
