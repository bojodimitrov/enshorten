using Dapper;
using System.Data.SqlClient;

namespace Enshorten
{
    public class OpenAddressingProbingQuery
    {
        private readonly string ConnectionString;

        public OpenAddressingProbingQuery(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(DatabaseContext.Enshorten);
        }

        public virtual async Task<IEnumerable<int>> ExecuteAscending(int pivot, int probeNumber)
        {
            using var connection = new SqlConnection(ConnectionString);
            var query = @"SELECT TOP (@probeNumber) ShortHash FROM [Enshorten].[dbo].[Urls] 
                          WHERE ShortHash > @pivot
                          ORDER BY ShortHash";
            return await connection.QueryAsync<int>(query, new { pivot, probeNumber });
        }

        public virtual async Task<IEnumerable<int>> ExecuteDescending(int pivot, int probeNumber)
        {
            using var connection = new SqlConnection(ConnectionString);
            var query = @"SELECT TOP (@probeNumber) ShortHash FROM [Enshorten].[dbo].[Urls] 
                          WHERE ShortHash < @pivot
                          ORDER BY ShortHash DESC";
            return await connection.QueryAsync<int>(query, new { pivot, probeNumber });
        }
    }
}
