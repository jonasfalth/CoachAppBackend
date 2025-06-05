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
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using CoachBackend;

Console.WriteLine("Starting application...");

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Configuring Serilog...");
// Konfigurera Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

Console.WriteLine("Adding services...");
// Lägg till services till containern
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Coach API", Version = "v1" });
    
    // Lägg till XML-kommentarer om de finns
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
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
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey is not configured");
}

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

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

// Konfigurera databasanslutningar med dependency injection
var userDbPath = builder.Configuration.GetValue<string>("Database:UserDbPath") ?? "UserDb.db";
var coachAppDbPath = builder.Configuration.GetValue<string>("Database:CoachAppDbPath") ?? "CoachAppDb.db";

Console.WriteLine($"Använder användardatabas: {userDbPath}");
Console.WriteLine($"Använder coach-app databas: {coachAppDbPath}");

// Skapa och initialisera användardatabasen
var userConnection = new SqliteConnection($"Data Source={userDbPath}");
Console.WriteLine("Öppnar användardatabasanslutning...");
userConnection.Open();
Console.WriteLine("Användardatabasanslutning öppnad.");

Console.WriteLine("Startar initialisering av användardatabasen...");
UserDatabaseInit.InitializeUserDatabase(userConnection);
Console.WriteLine("Användardatabasen initialiserad.");

// Registrera användardatabasanslutningen som singleton
builder.Services.AddSingleton<SqliteConnection>(userConnection);
builder.Services.AddSingleton<IDbConnection>(userConnection);

// Skapa och öppna coach-app databasanslutningen
var coachAppConnection = new SqliteConnection($"Data Source={coachAppDbPath}");
Console.WriteLine("Öppnar coach-app databasanslutning...");
coachAppConnection.Open();
Console.WriteLine("Coach-app databasanslutning öppnad.");

// Registrera coach-app databasanslutningen som singleton
builder.Services.AddSingleton<SqliteConnection>(coachAppConnection);
builder.Services.AddSingleton<IDbConnection>(coachAppConnection);

// Registrera repositories och services med rätt databasanslutning
builder.Services.AddScoped<UserRepository>(sp => new UserRepository(userConnection));
builder.Services.AddScoped<TeamRepository>(sp => new TeamRepository(userConnection));
builder.Services.AddScoped<UserTeamRepository>(sp => new UserTeamRepository(userConnection));
builder.Services.AddScoped<PlayerRepository>(sp => new PlayerRepository(coachAppConnection));
builder.Services.AddScoped<MatchRepository>(sp => new MatchRepository(coachAppConnection));
builder.Services.AddScoped<PositionRepository>(sp => new PositionRepository(coachAppConnection));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<PositionService>();

// Lägg till CORS med korrekt konfiguration för utveckling
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        corsBuilder
            .WithOrigins("http://localhost:3000", "https://localhost:3000") // Både HTTP och HTTPS
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Viktigt för JWT cookies
    });
});

// Lägg till TeamDatabaseConnectionManager som singleton
builder.Services.AddSingleton<TeamDatabaseConnectionManager>();

// Lägg till TeamDatabaseService
builder.Services.AddScoped<TeamDatabaseService>();

Console.WriteLine("Building application...");
var app = builder.Build();

Console.WriteLine("Configuring middleware...");
// Konfigurera HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Lägg till global felhantering
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            Log.Error(error.Error, "Unhandled exception");
            
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 500,
                Message = "Ett fel uppstod. Försök igen senare.",
                RequestId = context.TraceIdentifier
            });
        }
    });
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Aktivera TeamDatabaseMiddleware
app.UseMiddleware<TeamDatabaseMiddleware>();

app.MapControllers();

// Stäng databasanslutningar när applikationen avslutas
var lifetime = app.Lifetime;
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Stänger databasanslutningar...");
    
    // Stäng team-databaser
    var teamConnectionManager = app.Services.GetRequiredService<TeamDatabaseConnectionManager>();
    teamConnectionManager.Dispose();
    Console.WriteLine("Team-databasanslutningar stängda.");
    
    if (userConnection.State != ConnectionState.Closed)
    {
        userConnection.Close();
        Console.WriteLine("Användardatabasanslutning stängd.");
    }
    if (coachAppConnection.State != ConnectionState.Closed)
    {
        coachAppConnection.Close();
        Console.WriteLine("Coach-app databasanslutning stängd.");
    }
});

Console.WriteLine("Starting web server...");
app.Run(); 