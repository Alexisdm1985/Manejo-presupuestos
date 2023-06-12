using Dapper;
using manejo_presupuestos.Models;
using Microsoft.Data.SqlClient;
using manejo_presupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioTiposCuentas // Le dice a la clase que debe tener si o si estos metodos, pero la clase sabra su contenido.
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId);
        Task<TipoCuenta> ObtenerTipoDeCuenta(int id, int usuarioId);
        Task Actualizar(TipoCuenta tipoCuenta);
    }

    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        // ## PROPIEDADES
        private string connectionString;

        // ## CONSTRUCTOR
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ## METODOS

        //Obtener lista de los tipos de cuentas
        public async Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            var tiposCuenta = await cnn.QueryAsync<TipoCuenta>("SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId;",
                new { usuarioId });

            return tiposCuenta;
        }

        // Crea en la DB un registro de tipoCuenta
        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var cnn = new SqlConnection(connectionString);
            var idTipoCuenta = await cnn.QuerySingleAsync<int>("INSERT INTO TiposCuentas VALUES (@Nombre, @UsuarioId, 0);" +
                                                "SELECT SCOPE_IDENTITY();",
                                                tipoCuenta);
            tipoCuenta.Id = idTipoCuenta;
        }

        //Consulta a la base de datos si existe usuario
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

        //Actualiza tipo de cuenta
        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            var cnn = new SqlConnection(connectionString);
            await cnn.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre WHERE Id = @Id", tipoCuenta);
        }

        //Obtiene un tipo de cuenta
        public async Task<TipoCuenta> ObtenerTipoDeCuenta(int id, int usuarioId)
        {
            using var cnn = new SqlConnection(connectionString);
            var tipoCuenta = await cnn.QuerySingleAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE Id = @Id AND UsuarioId = @UsuarioId",
                new { id, usuarioId });

            return tipoCuenta;
        }
    }
}
