using Dapper;
using manejo_presupuestos.Models.Cuenta;
using Microsoft.Data.SqlClient;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task<IEnumerable<Cuentas>> BuscarCuentas(int usuarioId);
        Task Crear(Cuentas Cuenta);
        Task Eliminar(int id);
        Task<Cuentas> ObtenerCuentaPorId(int idCuenta, int usuarioId);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {

        private readonly string connectionString;
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public RepositorioCuentas(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios, IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task Crear(Cuentas cuenta)
        {
            int usuarioId = servicioUsuarios.ObtenerUsuarioId();
            using var cnn = new SqlConnection(connectionString);

            await cnn.QuerySingleOrDefaultAsync(@"INSERT INTO Cuentas VALUES(@Nombre, @TipoCuentaId, @Balance, @Descripcion);", cuenta);
        }

        public async Task<IEnumerable<Cuentas>> BuscarCuentas(int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            var cuentas = await cnn.QueryAsync<Cuentas>(@"SELECT cu.Id, cu.Nombre, cu.Balance,tc.Nombre AS TipoCuenta
                                                            FROM Cuentas cu
                                                            JOIN TiposCuentas tc ON cu.TipoCuentaId = tc.Id
                                                            WHERE tc.UsuarioId = @UsuarioId;", new { usuarioId });
            return cuentas;
        }

        public async Task<Cuentas> ObtenerCuentaPorId(int id, int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            return await cnn.QueryFirstOrDefaultAsync<Cuentas>(
                @"SELECT cu.Id, cu.Nombre, cu.Balance, cu.Descripcion, cu.TipoCuentaId
                FROM Cuentas cu
                JOIN TiposCuentas tc ON cu.TipoCuentaId = tc.Id
                WHERE tc.UsuarioId = @UsuarioId AND cu.Id = @Id;", new { usuarioId, id });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var cnn = new SqlConnection(connectionString);

            await cnn.ExecuteAsync(@"UPDATE Cuentas 
                                    SET Nombre = @Nombre, Balance = @Balance,
                                        Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
                                    WHERE Id = @Id",
                                        cuenta);
        }

        public async Task Eliminar(int id)
        {
            using var cnn = new SqlConnection(connectionString);
            await cnn.ExecuteAsync(@"DELETE Cuentas WHERE Id = @Id", new { id });
        }
    }
}
