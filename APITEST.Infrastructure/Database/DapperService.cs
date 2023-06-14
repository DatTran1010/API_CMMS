using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Infrastructure.Database
{
    public class DapperService : IDapperService
    {
        private readonly string Connectionstring;
        private readonly IConfiguration _config;

        public DapperService(IConfiguration config)
        {
            _config = config;
            Connectionstring = _config["ConnectionStrings:HRMConnection"];
        }

        public void Dispose()
        {

        }

        public async Task<T> Execute<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            IEnumerable<T>? res = await db.QueryAsync<T>(sp, parms, commandType: commandType);
            return res.First();
        }
        public async Task QueryMultipleAsync(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, Action<SqlMapper.GridReader> action = null)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            SqlMapper.GridReader? a = await db.QueryMultipleAsync(sp, parms, commandType: commandType);

            action?.Invoke(a);
        }
        
        public T? Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            return db.Query<T>(sp, parms, commandType: commandType).FirstOrDefault();

        }

        public async Task<List<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            IEnumerable<T>? res = await db.QueryAsync<T>(sp, parms, commandType: commandType);
            return res.ToList();
        }

        public DbConnection GetDbconnection()
        {
            return new SqlConnection(Connectionstring);
        }

        public async Task ExecuteNonQuery(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using IDbConnection db = new SqlConnection(Connectionstring);
            await db.ExecuteAsync(sp, parms, commandType: CommandType.StoredProcedure);
        }
    }
}
