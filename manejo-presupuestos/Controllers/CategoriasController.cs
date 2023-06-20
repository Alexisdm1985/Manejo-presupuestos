using manejo_presupuestos.Servicios;
using manejo_presupuestos.Models.Categorias;
using Microsoft.AspNetCore.Mvc;

namespace manejo_presupuestos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuarios servicioUsuarios;

        public CategoriasController(IRepositorioCategorias repositorioCategorias, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.ObtenerCategorias(usuarioId);

            return View(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            
            categoria.UsuarioId = usuarioId;

            await repositorioCategorias.Crear(categoria);

            return RedirectToAction("Index");
        }
    }
}
