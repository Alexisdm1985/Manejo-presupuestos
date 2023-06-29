using Dapper;
using manejo_presupuestos.Models.Transaccion;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task ActualizarTransaccion(decimal montoAnterior, int categoriaAnteriorId, Transaccion transaccion);
        Task<Transaccion> BuscarTransaccionPorId(int id, int usuarioId);
        Task CrearTransaccion(Transaccion transaccion);
        Task EliminarTransaccion(int id);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CrearTransaccion(Transaccion transaccion)
        {
            using var cnn = new SqlConnection(connectionString);
            var id = await cnn.QuerySingleAsync<int>("SP_INSERTAR_TRANSACCION",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Nota,
                    transaccion.CuentaId,
                    transaccion.CategoriaId,
                    transaccion.Monto
                }, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task ActualizarTransaccion(decimal montoAnterior, int cuentaAnteriorId, Transaccion transaccion)
        {
            using var cnn = new SqlConnection(connectionString);

            await cnn.ExecuteAsync("SP_ACTUALIZAR_TRANSACCION",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.UsuarioId,
                    montoAnterior,
                    cuentaAnteriorId

                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> BuscarTransaccionPorId(int id, int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT tr.*, ct.TipoOperacionesId 
                FROM Transacciones tr
                JOIN Categorias ct ON (ct.Id= tr.CategoriaId)
                WHERE tr.Id = @Id AND tr.UsuarioId = @UsuarioId;", new { id, usuarioId });
        }

        public async Task EliminarTransaccion(int id)
        {
            using var cnn = new SqlConnection(connectionString);

            await cnn.ExecuteAsync("SP_ELIMINAR_TRANSACCION", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }

}
