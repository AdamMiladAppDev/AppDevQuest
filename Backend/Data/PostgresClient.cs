using Microsoft.Extensions.Options;
using Npgsql;
using Backend.Settings;

namespace Backend.Data
{
    public class PostgresClient
    {
        private readonly string _connectionString;

        public PostgresClient(IOptions<DbSettings> settings)
        {
            _connectionString = settings.Value.DefaultConnection;
        }

        public NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
