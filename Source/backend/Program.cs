using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;
using Microsoft.Data.Sqlite;
using CoachBackend.Data;
using CoachBackend.Repositories;
using CoachBackend.Services;
using CoachBackend.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CoachBackend.Authentication;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using CoachBackend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Lägg till services till containern
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Coach API", Version = "v1" });
    
    // Lägg till XML-kommentarer
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Lägg till JWT-autentisering i Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Lägg till JWT-autentisering
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured"))),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Konfigurera för att hämta token från cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

// Registrera JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

// Registrera AuthenticationService
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Konfigurera UserDb
var userDbConnection = new SqliteConnection("Data Source=UserDb.sqlite");
userDbConnection.Open();
UserDatabaseInit.InitializeUserDatabase(userDbConnection);

// Registrera repositories och services för UserDb
builder.Services.AddScoped<UserRepository>(_ => new UserRepository(userDbConnection));
builder.Services.AddScoped<TeamRepository>(_ => new TeamRepository(userDbConnection));
builder.Services.AddScoped<UserTeamRepository>(_ => new UserTeamRepository(userDbConnection));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TeamService>();

// Konfigurera CoachAppDb
var coachAppDbConnection = new SqliteConnection("Data Source=CoachAppDb.db");
coachAppDbConnection.Open();
DatabaseInit.InitializeDatabase(coachAppDbConnection);

// Registrera repositories och services för CoachAppDb
builder.Services.AddScoped<PlayerRepository>(_ => new PlayerRepository(coachAppDbConnection));
builder.Services.AddScoped<MatchRepository>(_ => new MatchRepository(coachAppDbConnection));
builder.Services.AddScoped<PositionRepository>(_ => new PositionRepository(coachAppDbConnection));
// Tillfälligt kommenterat tills Gameplay-klasserna fungerar
//builder.Services.AddScoped<GameplayRepository>(_ => new GameplayRepository(coachAppDbConnection));
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<PositionService>();
//builder.Services.AddScoped<GameplayService>();

// Lägg till CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Lägg till TeamDatabaseService med UserDb connection
builder.Services.AddSingleton<TeamDatabaseService>(sp => new TeamDatabaseService(userDbConnection));

var app = builder.Build();

// Konfigurera HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(); // Lägg till CORS middleware
app.UseAuthentication();
app.UseAuthorization();

// Lägg till TeamDatabaseMiddleware
//app.UseMiddleware<TeamDatabaseMiddleware>();

app.MapControllers();

app.Run(); 