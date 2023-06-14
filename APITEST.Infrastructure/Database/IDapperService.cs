using Dapper;
using System.Data;
using System.Data.Common;

namespace APITEST.Infrastructure.Database
{
    public interface IDapperService 
    {
        DbConnection GetDbconnection();
        T? Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<List<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<T> Execute<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task ExecuteNonQuery(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task QueryMultipleAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, Action<SqlMapper.GridReader> action = null);
    }
}
