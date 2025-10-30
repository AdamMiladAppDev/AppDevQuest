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
CREATE INDEX IF NOT EXISTS idx_survey_invitations_survey_id ON survey_invitations (survey_id);
