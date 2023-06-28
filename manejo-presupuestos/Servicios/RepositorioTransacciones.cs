using Dapper;
using manejo_presupuestos.Models.Transaccion;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task CrearTransaccion(Transaccion transaccion);
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
    }

}
