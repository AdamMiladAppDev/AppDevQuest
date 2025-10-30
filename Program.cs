using Backend.Data;
using Backend.Repository;
using Backend.Services.Auth;
using Backend.Services.Email;
using Backend.Services.Security;
using Backend.Services.Surveys;
using Backend.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(nameof(ApplicationSettings)));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));

builder.Services.AddSingleton<PostgresClient>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var authSettings = builder.Configuration.GetSection(nameof(AuthSettings)).Get<AuthSettings>();
if (authSettings is null || string.IsNullOrWhiteSpace(authSettings.JwtSecret))
{
    throw new InvalidOperationException("AuthSettings:JwtSecret must be configured.");
}

if (Encoding.UTF8.GetByteCount(authSettings.JwtSecret) < 16)
{
    throw new InvalidOperationException("AuthSettings:JwtSecret must be at least 16 bytes. Provide a longer secret or use a base64 string.");
}

var keyBytes = Encoding.UTF8.GetBytes(authSettings.JwtSecret);
if (keyBytes.Length < 32)
{
    keyBytes = SHA256.HashData(keyBytes);
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.EnsureDatabaseAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
