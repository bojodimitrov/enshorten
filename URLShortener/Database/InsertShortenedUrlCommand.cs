using System.Data.SqlClient;
using Dapper;

namespace Enshorten
{
    public class InsertShortenedUrlCommand
    {
        private readonly string ConnectionString;
        public InsertShortenedUrlCommand(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(DatabaseContext.Enshorten);
        }

        public virtual async Task<int> Execute(ShortenedUrlDto shortenedUrl)
        {
            using var connection = new SqlConnection(ConnectionString);
            var query = @"INSERT INTO Urls (ShortHash, FullUrl) VALUES(@shortHash, @fullUrl)";
            await connection.ExecuteAsync(query, 
                new 
                { 
                    shortHash = shortenedUrl.Hash,
                    fullUrl = shortenedUrl.Url 
                });
            return shortenedUrl.Hash;
        }
    }
}
