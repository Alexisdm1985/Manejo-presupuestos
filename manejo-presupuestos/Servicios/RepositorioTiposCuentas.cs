using Dapper;
using manejo_presupuestos.Models;
using Microsoft.Data.SqlClient;
using manejo_presupuestos.Servicios;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private string connectionString;


        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var cnn = new SqlConnection(connectionString);
            var idTipoCuenta = await cnn.QuerySingleAsync<int>("INSERT INTO TiposCuentas VALUES (@Nombre, @UsuarioId, 0);" +
                                                "SELECT SCOPE_IDENTITY();",
                                                tipoCuenta);
            tipoCuenta.Id = idTipoCuenta;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            var existe = await cnn.QueryFirstOrDefaultAsync<int>(
                @"select 1 
                from TiposCuentas
                where Nombre = @Nombre 
                AND UsuarioId = @UsuarioId;", 
                new {nombre, usuarioId});

            return existe == 1; 
        }
    }
}
