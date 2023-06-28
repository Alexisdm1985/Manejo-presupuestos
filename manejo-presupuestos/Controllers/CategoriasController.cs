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

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerCategoria(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria nuevaCategoria)
        {
            if (!ModelState.IsValid)
            {
                return View(nuevaCategoria);
            }

            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerCategoria(nuevaCategoria.Id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            nuevaCategoria.UsuarioId = usuarioId;
            await repositorioCategorias.ActualizarCategoria(nuevaCategoria);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerCategoria(id, usuarioId);

            if (categoria is null) RedirectToAction("NoEncontrado", "Home");

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerCategoria(id, usuarioId);

            if (categoria is null) RedirectToAction("NoEncontrado", "Home");

            // eliminar categoria
            await repositorioCategorias.EliminarCategoria(categoria);

            return RedirectToAction("Index");
        }
    }
}
