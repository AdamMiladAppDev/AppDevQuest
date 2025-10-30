using Backend.Entities;
using Backend.Repository;
using Dapper;
using System.Data;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Backend.Data
{
    public class DatabaseInitializer
    {
        private readonly PostgresClient _client;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly IUserRepository _users;

        public DatabaseInitializer(PostgresClient client, IUserRepository users, ILogger<DatabaseInitializer> logger)
        {
            _client = client;
            _users = users;
            _logger = logger;
        }

        public async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id UUID PRIMARY KEY,
                    email TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );

                CREATE TABLE IF NOT EXISTS surveys (
                    id UUID PRIMARY KEY,
                    title TEXT NOT NULL,
                    description TEXT NULL,
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );

                CREATE TABLE IF NOT EXISTS survey_questions (
                    id UUID PRIMARY KEY,
                    survey_id UUID NOT NULL REFERENCES surveys(id) ON DELETE CASCADE,
                    prompt TEXT NOT NULL,
                    question_type TEXT NOT NULL DEFAULT 'text',
                    options_json TEXT NULL,
                    order_index INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS survey_invitations (
                    token_hash TEXT PRIMARY KEY,
                    survey_id UUID NOT NULL REFERENCES surveys(id) ON DELETE CASCADE,
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    expires_at TIMESTAMPTZ NULL,
                    responded_at TIMESTAMPTZ NULL
                );

                CREATE TABLE IF NOT EXISTS survey_responses (
                    id UUID PRIMARY KEY,
                    survey_id UUID NOT NULL REFERENCES surveys(id) ON DELETE CASCADE,
                    submitted_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    invitation_token_hash TEXT NOT NULL REFERENCES survey_invitations(token_hash),
                    CONSTRAINT survey_responses_unique_invitation UNIQUE (invitation_token_hash)
                );

                CREATE TABLE IF NOT EXISTS survey_answers (
                    id UUID PRIMARY KEY,
                    response_id UUID NOT NULL REFERENCES survey_responses(id) ON DELETE CASCADE,
                    question_id UUID NOT NULL REFERENCES survey_questions(id) ON DELETE CASCADE,
                    answer_text TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_survey_questions_survey_id ON survey_questions (survey_id);
                CREATE INDEX IF NOT EXISTS idx_survey_answers_response_id ON survey_answers (response_id);
                CREATE INDEX IF NOT EXISTS idx_survey_invitations_survey_id ON survey_invitations (survey_id);";

            var attempt = 0;
            const int maxAttempts = 5;
            Exception? lastException = null;

            while (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    attempt++;
                    await using var connection = _client.GetConnection();
                    await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
                    await SeedDefaultUserAsync(connection, cancellationToken);
                    _logger.LogInformation("Database schema ensured.");
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    lastException = ex;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogWarning(ex, "Database initialization attempt {Attempt} failed. Retrying in {Delay}...", attempt, delay);
                    await Task.Delay(delay, cancellationToken);
                }
            }

            _logger.LogCritical(lastException, "Unable to initialize database after {Attempts} attempts.", maxAttempts);
            throw lastException ?? new InvalidOperationException("Database initialization failed.");
        }

        private async Task SeedDefaultUserAsync(IDbConnection connection, CancellationToken cancellationToken)
        {
            const string email = "admin@example.com";
            const string defaultPassword = "ChangeMe123!";

            const string existsSql = "SELECT 1 FROM users WHERE LOWER(email) = LOWER(@Email) LIMIT 1;";
            var exists = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(existsSql, new { Email = email }, cancellationToken: cancellationToken));

            if (exists.HasValue)
            {
                return;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCryptNet.HashPassword(defaultPassword),
                CreatedAt = DateTime.UtcNow
            };

            await _users.EnsureDefaultUserAsync(user, cancellationToken);
            _logger.LogInformation("Seeded default administrator user {Email}.", email);
        }
    }
}
