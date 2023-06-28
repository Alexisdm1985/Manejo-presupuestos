using manejo_presupuestos.Models.Categorias;
using manejo_presupuestos.Models.Transaccion;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            modelo.Categorias = await Obtenercategorias(usuarioId, modelo.TipoOperacionId);

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
                transaccion.Categorias = await Obtenercategorias(usuarioId, transaccion.TipoOperacionId);

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

        public async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.BuscarCuentas(usuarioId);

            if (cuentas is null) RedirectToAction("NoEncontrado", "Home");

            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }

        private async Task<IEnumerable<SelectListItem>> Obtenercategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.ObtenerPorTipoOperacion(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        //Peticion desde JS en la vista
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await Obtenercategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}
