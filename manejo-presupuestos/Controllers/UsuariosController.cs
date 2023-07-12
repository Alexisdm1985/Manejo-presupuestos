using manejo_presupuestos.Models;
using manejo_presupuestos.Models.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace manejo_presupuestos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;

        public UsuariosController(UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager) // SignInManager es para crear la coockie de logueo usuario
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modeloUsuario)
        {
            if (!ModelState.IsValid)
            {
                return View(modeloUsuario);
            }

            var nuevoUsuario = new Usuario()
            {
                Email = modeloUsuario.Email,
                PasswordHash = modeloUsuario.Contrasenia
            };

            var resultado = await userManager.CreateAsync(nuevoUsuario, password: modeloUsuario.Contrasenia);

            if (resultado.Succeeded)
            {
                // coockie logueo usuario
                await signInManager.SignInAsync(nuevoUsuario, isPersistent: true);

                return RedirectToAction("Index", "Transaccion");

            } else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modeloUsuario);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transaccion");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginModelo)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModelo);
            }

            var resultado = await signInManager.PasswordSignInAsync(loginModelo.Email, loginModelo.Contrasenia, loginModelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transaccion");
            } else
            {
                ModelState.AddModelError(String.Empty, "Nombre de usuario o password incorrecto.");
                return View(loginModelo);
            }
        }
    }
}
