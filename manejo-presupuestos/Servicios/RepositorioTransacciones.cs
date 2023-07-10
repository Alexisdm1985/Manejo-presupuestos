using Dapper;
using manejo_presupuestos.Models.Transaccion;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace manejo_presupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task ActualizarTransaccion(decimal montoAnterior, int categoriaAnteriorId, Transaccion transaccion);
        Task<Transaccion> BuscarTransaccionPorId(int id, int usuarioId);
        Task CrearTransaccion(Transaccion transaccion);
        Task EliminarTransaccion(int id);
        Task<IEnumerable<ReporteTransaccionesPorMes>> ObtenerDetallePorMes(int usuarioId, int anio);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerDetallePorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionPorCuenta(TransaccionPorTipoCuentaViewModel modelo);
        Task<IEnumerable<Transaccion>> ObtenerTransaccionPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
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

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionPorCuenta(TransaccionPorTipoCuentaViewModel modelo)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QueryAsync<Transaccion>(
                @"SELECT t.FechaTransaccion, t.Monto, cu.Nombre as Cuenta, ca.Nombre as Categoria, ca.TipoOperacionesId, cu.TipoCuentaId, t.Id
                    FROM Transacciones t
                    JOIN Cuentas cu ON (cu.Id = t.CuentaId)
                    JOIN Categorias ca ON (ca.Id = t.CategoriaId)
                    WHERE t.UsuarioId = @UsuarioId AND t.CuentaId = @CuentaId
                    AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QueryAsync<Transaccion>(
                @"SELECT t.FechaTransaccion, t.Monto, cu.Nombre as Cuenta, ca.Nombre as Categoria, ca.TipoOperacionesId, t.Id
                    FROM Transacciones t
                    JOIN Cuentas cu ON (cu.Id = t.CuentaId)
                    JOIN Categorias ca ON (ca.Id = t.CategoriaId)
                    WHERE t.UsuarioId = @UsuarioId 
                    AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY t.FechaTransaccion DESC;", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerDetallePorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QueryAsync<ResultadoObtenerPorSemana>(
                @"SELECT 
	                DATEDIFF(d, @FechaInicio, FechaTransaccion)/7 + 1 AS Semana,
	                SUM(Monto) AS Monto,
	                cat.TipoOperacionesId
                FROM Transacciones
                JOIN Categorias cat ON cat.Id = Transacciones.CategoriaId
                WHERE FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                AND Transacciones.UsuarioId = @UsuarioId
                GROUP BY DATEDIFF(d, @FechaInicio, FechaTransaccion)/7, cat.TipoOperacionesId;", modelo);
        }

        public async Task<IEnumerable<ReporteTransaccionesPorMes>> ObtenerDetallePorMes(int usuarioId, int anio)
        {
            using var cnn = new SqlConnection(connectionString);

            return await cnn.QueryAsync<ReporteTransaccionesPorMes>(@"
                        SELECT
	                        MONTH(tr.FechaTransaccion) AS Mes,
	                        SUM(Monto) AS Monto,
	                        cat.TipoOperacionesId AS TipoOperacionId
                        FROM Transacciones tr
                        JOIN Categorias cat ON cat.Id = tr.CategoriaId
                        WHERE tr.UsuarioId = @UsuarioId
                        AND YEAR(tr.FechaTransaccion) = @Anio
                        GROUP BY tr.FechaTransaccion, cat.TipoOperacionesId;", new { usuarioId, anio });
        }
    }

}
