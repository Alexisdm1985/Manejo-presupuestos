using Dapper;
using manejo_presupuestos.Models.Usuarios;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Servicios
{

    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var cnn = new SqlConnection(connectionString);

            var usuarioId = await cnn.QuerySingleAsync<int>(
                @"INSERT INTO Usuarios (Email, PasswordHash, EmailNormalizado)
                    VALUES(@Email, @PasswordHash, @EmailNormalizado);
                SELECT SCOPE_IDENTITY();", usuario);

            await cnn.ExecuteAsync("CargarDatosPorDefectoNuevoUsuario", 
                new { usuarioId }, 
                commandType: System.Data.CommandType.StoredProcedure);

            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QuerySingleOrDefaultAsync<Usuario>(@"SELECT * FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado",
                    new { emailNormalizado });
        }
    }
}
