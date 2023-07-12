using System.Security.Claims;

namespace manejo_presupuestos.Servicios
{
    public interface IServicioUsuarios
    {
        public int ObtenerUsuarioId();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccesor)
        {
            this.httpContext = httpContextAccesor.HttpContext;
        }

        public int ObtenerUsuarioId()
        {

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse(idClaim.Value);
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no esta autenticado");
            }
        }
    }
}
