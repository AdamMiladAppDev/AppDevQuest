using Backend.Data;
using Backend.Entities;
using Dapper;
using System.Linq;

namespace Backend.Repository
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly PostgresClient _client;

        public SurveyRepository(PostgresClient client)
        {
            _client = client;
        }

        public async Task<Guid> CreateSurveyAsync(
            Survey survey,
            IEnumerable<SurveyQuestion> questions,
            CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            const string insertSurveySql = @"
                INSERT INTO surveys (id, title, description, created_at)
                VALUES (@Id, @Title, @Description, @CreatedAt);";

            await connection.ExecuteAsync(new CommandDefinition(
                insertSurveySql,
                new
                {
                    survey.Id,
                    survey.Title,
                    survey.Description,
                    survey.CreatedAt
                },
                transaction,
                cancellationToken: cancellationToken));

            const string insertQuestionSql = @"
                INSERT INTO survey_questions (id, survey_id, prompt, question_type, options_json, order_index)
                VALUES (@Id, @SurveyId, @Prompt, @QuestionType, @OptionsJson, @OrderIndex);";

            foreach (var question in questions)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    insertQuestionSql,
                    new
                    {
                        question.Id,
                        question.SurveyId,
                        question.Prompt,
                        question.QuestionType,
                        question.OptionsJson,
                        OrderIndex = question.Order
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return survey.Id;
        }

        public async Task<IReadOnlyCollection<Survey>> GetAllSurveysAsync(CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();

            const string surveySql = @"
                SELECT id, title, description, created_at
                FROM surveys
                ORDER BY created_at DESC;";

            var surveys = (await connection.QueryAsync<Survey>(
                new CommandDefinition(surveySql, cancellationToken: cancellationToken))).ToList();

            if (!surveys.Any())
            {
                return surveys;
            }

            var surveyIds = surveys.Select(s => s.Id).ToArray();

            const string questionSql = @"
                SELECT id,
                       survey_id AS SurveyId,
                       prompt,
                       question_type AS QuestionType,
                       options_json AS OptionsJson,
                       order_index AS ""Order""
                FROM survey_questions
                WHERE survey_id = ANY(@SurveyIds)
                ORDER BY order_index ASC;";

            var questions = await connection.QueryAsync<SurveyQuestion>(
                new CommandDefinition(
                    questionSql,
                    new { SurveyIds = surveyIds },
                    cancellationToken: cancellationToken));

            var questionsLookup = questions.GroupBy(q => q.SurveyId)
                                            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<SurveyQuestion>)g.ToList());

            foreach (var survey in surveys)
            {
                survey.Questions = questionsLookup.GetValueOrDefault(survey.Id) ?? Array.Empty<SurveyQuestion>();
            }

            return surveys;
        }

        public async Task<Survey?> GetSurveyAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();

            const string surveySql = @"
                SELECT id, title, description, created_at
                FROM surveys
                WHERE id = @SurveyId;";

            var survey = await connection.QueryFirstOrDefaultAsync<Survey>(
                new CommandDefinition(surveySql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));

            if (survey is null)
            {
                return null;
            }

            const string questionSql = @"
                SELECT id,
                       survey_id AS SurveyId,
                       prompt,
                       question_type AS QuestionType,
                       options_json AS OptionsJson,
                       order_index AS ""Order""
                FROM survey_questions
                WHERE survey_id = @SurveyId
                ORDER BY order_index ASC;";

            var questions = await connection.QueryAsync<SurveyQuestion>(
                new CommandDefinition(questionSql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));

            survey.Questions = questions.ToList();
            return survey;
        }

        public async Task<Survey?> GetSurveyByInvitationHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();

            const string sql = @"
                SELECT s.id, s.title, s.description, s.created_at
                FROM survey_invitations i
                INNER JOIN surveys s ON s.id = i.survey_id
                WHERE i.token_hash = @TokenHash;";

            var survey = await connection.QueryFirstOrDefaultAsync<Survey>(
                new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));

            if (survey is null)
            {
                return null;
            }

            const string questionSql = @"
                SELECT id,
                       survey_id AS SurveyId,
                       prompt,
                       question_type AS QuestionType,
                       options_json AS OptionsJson,
                       order_index AS ""Order""
                FROM survey_questions
                WHERE survey_id = @SurveyId
                ORDER BY order_index ASC;";

            var questions = await connection.QueryAsync<SurveyQuestion>(
                new CommandDefinition(questionSql, new { SurveyId = survey.Id }, cancellationToken: cancellationToken));

            survey.Questions = questions.ToList();
            return survey;
        }

        public async Task<(int invitations, int responses)> GetSurveyStatsAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM survey_invitations WHERE survey_id = @SurveyId) AS Invitations,
                    (SELECT COUNT(*) FROM survey_responses WHERE survey_id = @SurveyId) AS Responses;";

            var result = await connection.QuerySingleAsync<(int invitations, int responses)>(
                new CommandDefinition(sql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));

            return result;
        }

        public async Task<int> GetInvitationCountAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"SELECT COUNT(*) FROM survey_invitations WHERE survey_id = @SurveyId;";
            return await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));
        }

        public async Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"SELECT COUNT(*) FROM survey_responses WHERE survey_id = @SurveyId;";
            return await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));
        }

        public async Task AddInvitationAsync(SurveyInvitation invitation, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"
                INSERT INTO survey_invitations (token_hash, survey_id, created_at, expires_at)
                VALUES (@TokenHash, @SurveyId, @CreatedAt, @ExpiresAt);";

            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    invitation.TokenHash,
                    invitation.SurveyId,
                    invitation.CreatedAt,
                    invitation.ExpiresAt
                },
                cancellationToken: cancellationToken));
        }

        public async Task<SurveyInvitation?> GetInvitationByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"
                SELECT token_hash AS TokenHash,
                       survey_id AS SurveyId,
                       created_at AS CreatedAt,
                       expires_at AS ExpiresAt,
                       responded_at AS RespondedAt
                FROM survey_invitations
                WHERE token_hash = @TokenHash;";

            return await connection.QueryFirstOrDefaultAsync<SurveyInvitation>(
                new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));
        }

        public async Task MarkInvitationRespondedAsync(string tokenHash, DateTime respondedAt, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"
                UPDATE survey_invitations
                SET responded_at = @RespondedAt
                WHERE token_hash = @TokenHash;";

            await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { TokenHash = tokenHash, RespondedAt = respondedAt },
                cancellationToken: cancellationToken));
        }

        public async Task<Guid> SaveResponseAsync(SurveyResponse response, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            const string insertResponseSql = @"
                INSERT INTO survey_responses (id, survey_id, submitted_at, invitation_token_hash)
                VALUES (@Id, @SurveyId, @SubmittedAt, @InvitationTokenHash);";

            await connection.ExecuteAsync(new CommandDefinition(
                insertResponseSql,
                new
                {
                    response.Id,
                    response.SurveyId,
                    response.SubmittedAt,
                    response.InvitationTokenHash
                },
                transaction,
                cancellationToken: cancellationToken));

            const string insertAnswerSql = @"
                INSERT INTO survey_answers (id, response_id, question_id, answer_text)
                VALUES (@Id, @ResponseId, @QuestionId, @AnswerText);";

            foreach (var answer in response.Answers)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    insertAnswerSql,
                    new
                    {
                        answer.Id,
                        answer.ResponseId,
                        answer.QuestionId,
                        answer.AnswerText
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return response.Id;
        }

        public async Task<IReadOnlyCollection<SurveyInvitation>> GetInvitationsAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            await using var connection = _client.GetConnection();
            const string sql = @"
                SELECT token_hash AS TokenHash,
                       survey_id AS SurveyId,
                       created_at AS CreatedAt,
                       expires_at AS ExpiresAt,
                       responded_at AS RespondedAt
                FROM survey_invitations
                WHERE survey_id = @SurveyId;";

            var invitations = await connection.QueryAsync<SurveyInvitation>(
                new CommandDefinition(sql, new { SurveyId = surveyId }, cancellationToken: cancellationToken));

            return invitations.ToList();
        }
    }
}
