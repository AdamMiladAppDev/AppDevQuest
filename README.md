# Dev Quest – Survey Management Tool

This project implements an end-to-end survey management platform built for the App Dev Quest challenge. Administrators can create multi-question surveys, send unique invitations to recipients via email, and track participation. Respondents submit fully anonymous answers through single-use links – no identifying information is ever stored alongside a response.

## Key Capabilities

- Securely sign in before managing surveys (JWT-based authentication).
- Create surveys with any number of free-form questions.
- Generate and email single-use response links to recipients directly from the app.
- Enforce “one response per invite” while keeping submissions anonymous.
- Track invitations sent and responses received at a glance.
- Anonymous response portal that works on desktop and mobile.
- Offline mini-game keeps users engaged when connectivity drops.
- Automated schema bootstrapping for PostgreSQL via Dapper.
- Docker Compose stack (frontend + backend + PostgreSQL) and GitHub Actions CI pipeline.

## Architecture Overview

- **Frontend**: React (Vite) single page application styled with MUI. Includes login, dashboard, respondent view, and an offline mini-game.
- **Backend**: .NET 8 Web API secured with JWT bearer authentication. Uses Dapper for data access and Npgsql for PostgreSQL connectivity. Contains services for survey orchestration, invitation management, and SMTP/file-drop email delivery.
- **Database**: PostgreSQL with five tables (`surveys`, `survey_questions`, `survey_invitations`, `survey_responses`, `survey_answers`). Schema is created automatically at runtime (see `docs/schema.sql` for a reference script).
- **Email delivery**: Pluggable SMTP sender. In development, emails can be written to a local drop folder instead of being sent, making it easy to observe invite contents.
- **CI/CD**: GitHub Actions workflow (`.github/workflows/ci.yml`) builds both backend and frontend on every push/PR. Deployment instructions for Azure are included below.

### Anonymity & One-Response Guarantee

1. When invitations are sent, the backend generates a cryptographically strong token for each recipient.
2. Only the SHA-256 hash of that token is stored in the `survey_invitations` table; the email address is discarded immediately after the message is handed off to SMTP (or written to the drop folder).
3. Respondents access the survey via `/respond/{token}`. The backend hashes the provided token, validates it against the invitation, and loads the survey questions.
4. Upon submission, responses are stored alongside the invitation hash (never the raw token) and the invitation is marked as used. A unique constraint on `survey_responses.invitation_token_hash` guarantees “exactly one response per invite”.

This design ensures submissions cannot be linked back to email addresses or any personal data, while still preventing duplicate entries.

## Getting Started

### Prerequisites

- .NET SDK 8.0+
- Node.js 20+
- PostgreSQL 14+ (Docker Compose provides a ready-to-use instance)
- Docker (optional but recommended for an all-in-one run)

### Configuration

Backend configuration lives in `Backend/appsettings.json`:

- `DbSettings.DefaultConnection`: Connection string for PostgreSQL.
- `EmailSettings`: SMTP configuration. Set `OverrideDropDirectory` (default `EmailDrop`) to capture invitation emails as files during development.
- `ApplicationSettings.ResponseBaseUrl`: Base URL used when building the public response links (e.g. `https://yourfrontend.azurewebsites.net/respond`). Update this after deploying the frontend.
- `AuthSettings.JwtSecret`: Secret used to sign JWT access tokens for admin users (replace for any real deployment). Use at least 16 random bytes (32+ recommended); non-compliant values are rejected at startup.

Environment-specific overrides can be placed in `appsettings.Development.json` or environment variables (e.g. `EmailSettings__SmtpHost`).

### Running with Docker Compose

```bash
docker compose up --build
```

Services:

- Frontend: http://localhost:5173
- Backend API: http://localhost:8080 (proxied through the frontend at `/api`)
- PostgreSQL: port 5432 (credentials: `postgres` / `postgres`)

Invitation emails written via the file-dropper appear inside the backend container at `/app/EmailDrop`. Mount a volume or update `OverrideDropDirectory` to inspect them from the host.

### Running Locally (without Docker)

1. **Database** – start PostgreSQL locally and ensure the `appdb` database exists.
2. **Backend**
   ```bash
   cd Backend
   dotnet restore
   dotnet run
   ```
3. **Frontend**
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```
4. Open http://localhost:5173 to access the dashboard. The Vite dev server proxies `/api` requests to http://localhost:8080.

### Authentication & Default Credentials

- Startup seeds an initial administrator account so you can sign in immediately:
  - Email: `admin@example.com`
  - Password: `ChangeMe123!`
- Change these credentials before going live by inserting a new user (hashed password) or extending the API.
- Survey management endpoints (`/api/surveys/*`) require a valid JWT; the React client stores and refreshes authentication automatically after login.

### Offline Experience

- The SPA listens for browser online/offline events.
- When offline, dashboard/login routes switch to the **Offline Survey Quest** trivia mini-game.
- Respondents can continue drafting answers while offline and submit successfully once they reconnect.

## Deployment Notes (Azure)

### Backend

- Deploy the `Backend` project as an Azure App Service (Linux, .NET 8).
- Set the following App Settings:
  - `DbSettings__DefaultConnection` referencing Azure Database for PostgreSQL.
  - `EmailSettings__SmtpHost`, `EmailSettings__SmtpPort`, `EmailSettings__SmtpUsername`, `EmailSettings__SmtpPassword`, `EmailSettings__FromAddress`.
  - `ApplicationSettings__ResponseBaseUrl` pointing to your frontend respond route (e.g. `https://your-frontend.azurestaticapps.net/respond`).
- Ensure outbound SMTP is permitted (using SendGrid, MailJet, or Azure Communication Services SMTP credentials works well).

### Frontend

- Build the static assets (`npm run build`) and deploy the `Frontend/dist` directory to Azure Static Web Apps or Azure Storage Static Website hosting.
- Update the backend CORS configuration if serving from a different domain (see `Program.cs->AddCors`).
- If serving from Static Web Apps, configure a route rule to rewrite all requests to `/index.html`, and proxy `/api/*` to the backend App Service.

### CI/CD

- The included GitHub Action (`ci.yml`) compiles both parts of the application. Extend it with deployment jobs (Azure Web Apps Deploy, Azure Static Web Apps Deploy) using publish profiles or workload identities.

## Testing the Flow

1. Create a survey from the dashboard and add questions.
2. Send invitations to test email addresses; in development, check the `EmailDrop` folder for a `.txt` file containing the single-use link.
3. Open the provided `/respond/{token}` link in a browser, answer all questions, and submit.
4. Return to the dashboard to verify the response and invitation counts have updated.

## Repository Structure

```
Backend/          .NET 8 Web API + application services
Frontend/         React application (Vite + MUI) + Docker assets
docs/schema.sql   Database reference schema
docker-compose.yml
.github/workflows/ci.yml
```

## Future Enhancements

- Expand question types (multiple choice, ratings) and response visualisations.
- Add survey scheduling and automatic reminder emails.
- Include admin authentication/authorization.
- Implement export of aggregated response data.
- Extend CI pipeline with automated end-to-end tests.

---

Feel free to adjust configuration or styling to match your organisation’s branding and deployment environment.
