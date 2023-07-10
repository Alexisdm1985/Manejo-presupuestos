using manejo_presupuestos.Models.Cuenta;
using manejo_presupuestos.Models.Transaccion;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;

        public CuentasController(
            IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios,
            IRepositorioCuentas repositorioCuentas,
            IRepositorioTransacciones repositorioTransacciones,
            IServicioReportes servicioReportes) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
        }

     
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerListItemsTipoCuenta(usuarioId);   
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();

            // Valida que no exista 
            var tipoCuenta = repositorioTiposCuentas.ObtenerTipoDeCuenta(cuenta.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            // Valida el modelo
            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerListItemsTipoCuenta(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = servicioUsuarios.ObtenerUsuarioId();
            var cuentas = await repositorioCuentas.BuscarCuentas(userId); //Output: An IEnumarable of Cuentas.
            var modelo = cuentas
                    .GroupBy(x => x.TipoCuenta) //Return an object with the grouped property as KEY
                    .Select(c => new IndexCuentasViewModel
                    {
                        TipoCuenta = c.Key, // KEY = TipoCuenta
                        Cuentas = c.AsEnumerable()
                    }).ToList();


            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar (int idCuenta)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(idCuenta, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            };

            // Creacion del modelo para la vista
            var modelo = new CuentaCreacionViewModel()
            {
                Id = cuenta.Id,
                Nombre = cuenta.Nombre,
                Balance = cuenta.Balance,
                Descripcion = cuenta.Descripcion,
                TipoCuentaId = cuenta.TipoCuentaId
            };

            modelo.TiposCuentas = await ObtenerListItemsTipoCuenta(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar (CuentaCreacionViewModel cuentaEditar)
        {
            // Validaciones
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(cuentaEditar.Id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTiposCuentas.ObtenerTipoDeCuenta(cuentaEditar.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            //Actualizacion
            await repositorioCuentas.Actualizar(cuentaEditar);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar (int idCuenta)
        {
            // Validaciones
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(idCuenta, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCuenta(int id)
        {
            // Validaciones
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Eliminar(id);

            return RedirectToAction("Index");
        }

        // Obtiene los items del select para la creacion de tipos de cuentas 
        private async Task<IEnumerable<SelectListItem>> ObtenerListItemsTipoCuenta (int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


        // REPORTES
        public async Task<IActionResult> Detalle(int id, int mes, int anio)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            
            //Validaciones
            var cuenta = await repositorioCuentas.ObtenerCuentaPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, id, mes, anio, ViewBag);

            return View(modelo);
        }


    }
}
