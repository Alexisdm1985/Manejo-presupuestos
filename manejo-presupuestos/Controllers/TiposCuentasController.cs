using Dapper;
using manejo_presupuestos.Models;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
        }

        public IActionResult Crear()
        {

            return View();
        }

        [HttpPost] // Crear tipo cuenta
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            //Valida que tenga los datos requeridos
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = 1;

            //Si existe retorna error
            var existeTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            if (existeTipoCuenta)
            {
                // Nombre campo , Mensaje de error
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe.");

                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);

            return View();
        }

        [HttpGet] // Consultar si un tipo cuenta existe
        public async Task<IActionResult> ExisteTipocuenta(string nombre)
        {
            var existeTipoCuenta = await repositorioTiposCuentas.Existe(nombre, 1); // UsuarioId = 1

            if (existeTipoCuenta)
            {
                return Json($"El tipo de cuenta: {nombre}, ya existe en la base de datos");
            }

            return Json(true);
        }
    }
}
