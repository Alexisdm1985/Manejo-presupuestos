using Dapper;
using manejo_presupuestos.Models.Categorias;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioCategorias
    {
        Task ActualizarCategoria(Categoria categoria);
        Task Crear(Categoria categoria);
        Task Editar(Categoria categoria);
        Task EliminarCategoria(Categoria categoria);
        Task<Categoria> ObtenerCategoria(int id, int usuarioId);
        Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId);
        Task<IEnumerable<Categoria>> ObtenerPorTipoOperacion(int usuarioId, TipoOperacion tipoOperacionesId);
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

        public async Task Editar(Categoria categoria)
        {
            using var cnn = new SqlConnection(connectionString);
            await cnn.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionesId = @TipoOperacionesId,
                                                            UsuarioId = @UsuarioId;", categoria);

        }

        public async Task<Categoria> ObtenerCategoria(int id, int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            return await cnn.QueryFirstOrDefaultAsync<Categoria>(@"SELECT * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId;", new {id, usuarioId});
        }

        public async Task ActualizarCategoria(Categoria categoria)
        {
            using var cnn = new SqlConnection( connectionString);
            await cnn.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacionesId = @TipoOperacionesId
                                    WHERE UsuarioId = @UsuarioId AND Id = @Id;", categoria);
        }

        public async Task EliminarCategoria(Categoria categoria)
        {
            using var cnn = new SqlConnection(connectionString);

            await cnn.ExecuteAsync(@"DELETE Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId;", categoria);
        }

        public async Task<IEnumerable<Categoria>> ObtenerPorTipoOperacion(int usuarioId, TipoOperacion tipoOperacionesId)
        {
            using var cnn = new SqlConnection(connectionString);
            return await cnn.QueryAsync<Categoria>(@"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId AND TipoOperacionesId = @tipoOperacionesId;", 
                new { usuarioId, tipoOperacionesId });
        }
    }
}
