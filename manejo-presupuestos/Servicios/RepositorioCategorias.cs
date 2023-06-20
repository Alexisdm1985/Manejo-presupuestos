using Dapper;
using manejo_presupuestos.Models.Categorias;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {

        private readonly string connectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var cnn = new SqlConnection(connectionString);
            await cnn.ExecuteAsync(@"INSERT INTO Categorias VALUES(@Nombre, @TipoOperacionesId, @UsuarioId);", categoria);
        }

        public async Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            return await cnn.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId;", new {usuarioId});
        }
    }
}
