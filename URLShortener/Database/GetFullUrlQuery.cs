using Dapper;
using System.Data.SqlClient;

namespace Enshorten
{
    public class GetFullUrlQuery
    {
        private readonly string ConnectionString;
        public GetFullUrlQuery(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(DatabaseContext.Enshorten);
        }

        public virtual async Task<string> Execute(int hash)
        {
            using var connection = new SqlConnection(ConnectionString);
            var query = "SELECT FullUrl FROM Urls WHERE ShortHash = @hash";
            var url = await connection.QuerySingleAsync<string>(query, new { hash });
            return url;
        }
    }
}
