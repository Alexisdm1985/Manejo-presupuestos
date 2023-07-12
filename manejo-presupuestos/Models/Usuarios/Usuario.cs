namespace manejo_presupuestos.Models.Usuarios
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string EmailNormalizado => Email.ToUpper();
    }
}
