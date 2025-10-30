using Backend.Data;
using Backend.Entities;
using Dapper;

namespace Backend.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly PostgresClient _client;

        public UserRepository(PostgresClient client)
        {
            _client = client;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT id, email, password_hash AS PasswordHash, created_at AS CreatedAt
                                  FROM users
                                  WHERE LOWER(email) = LOWER(@Email)
                                  LIMIT 1;";

            await using var connection = _client.GetConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
        }

        public async Task EnsureDefaultUserAsync(User user, CancellationToken cancellationToken)
        {
            const string sql = @"INSERT INTO users (id, email, password_hash, created_at)
                                  VALUES (@Id, @Email, @PasswordHash, @CreatedAt)
                                  ON CONFLICT (email) DO NOTHING;";

            await using var connection = _client.GetConnection();
            await connection.ExecuteAsync(new CommandDefinition(sql,
                new
                {
                    user.Id,
                    user.Email,
                    user.PasswordHash,
                    user.CreatedAt
                },
                cancellationToken: cancellationToken));
        }
    }
}
