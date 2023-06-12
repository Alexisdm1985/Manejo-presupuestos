using Dapper;
using manejo_presupuestos.Models;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        // ## PROPIEDADES
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        // # CONSTRUCTOR
        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        // ## METODOS

        //Muestra vista de lista de tiposCuentas
        public async Task<IActionResult> Index() //Nota: Por convencion se llama Index la vista que muestra un listado de objetos.
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);
            return View(tiposCuentas);

        }

        //Muestra la vista de editar tipoCuenta
        [HttpGet]
        public async Task<IActionResult> Editar (int id)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerTipoDeCuenta(id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home"); //Action - Controller
            }

            return View(tipoCuenta);
        }

        //Actualiza el tipo de cuenta
        [HttpPost]
        public async Task<IActionResult> Editar (TipoCuenta tipoCuenta)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var existe = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, usuarioId);

            if (existe == true)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);

            return RedirectToAction("Index");
        }

        //Muestra la vista crear (crear formulario)
        public IActionResult Crear()
        {
            return View();
        }

        //Crear tipo cuenta
        [HttpPost] 
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            //Valida que tenga los datos requeridos
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId(); ;

            //Si existe retorna error
            var existeTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (existeTipoCuenta)
            {
                //Nombre campo , Mensaje de error
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");

                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        //Consulta si un tipo cuenta existe
        [HttpGet] //Nota: Este metodo se llama en el mismo modelo con un Remote
        public async Task<IActionResult> ExisteTipocuenta(string nombre)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var existeTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId); //UsuarioId = 1

            if (existeTipoCuenta)
            {
                return Json($"El tipo de cuenta: {nombre}, ya existe en la base de datos");
            }

            return Json(true);
        }

        [HttpGet]
        public async Task<TipoCuenta> ObtenerTipoCuenta(int id, int usuarioId)
        {
            return await repositorioTiposCuentas.ObtenerTipoDeCuenta(id, usuarioId);
        }
    }
}
