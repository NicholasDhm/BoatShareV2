using Microsoft.EntityFrameworkCore;
using boat_share.Data;
using boat_share.Abstract;
using boat_share.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework - use in-memory DB for Testing, SQLite for Development, PostgreSQL for Production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("BoatShare_TestDB_Program");
    }
    else if (builder.Environment.IsDevelopment())
    {
        // Use SQLite for local development (easier setup, no external dependencies)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=boatshare.db";
        options.UseSqlite(connectionString);
    }
    else
    {
        // Use PostgreSQL for production
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured");

        // Convert PostgreSQL URI format (from Railway) to Npgsql format if needed
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            connectionString = ConvertPostgresUrlToConnectionString(connectionString);
        }

        options.UseNpgsql(connectionString);
    }
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBoatService, BoatService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Register background services
builder.Services.AddHostedService<ReservationCleanupBackgroundService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        // Get allowed origins from environment variable or use defaults
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
            ?? new[] { "http://localhost:4200" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT secret key not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BoatShare",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BoatShare",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to use Brazil timezone for all DateTime values
        options.JsonSerializerOptions.Converters.Add(new boat_share.Converters.BrazilTimeZoneDateTimeConverter());
    });

// Configure port for Railway (uses PORT env var)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline

// Add global exception handler
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<boat_share.Middleware.GlobalExceptionMiddleware>();
}

app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize database - use migrations instead of EnsureCreated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
        Console.WriteLine("Note: If database doesn't exist, create it manually first.");
        // Don't throw - let the app start anyway
    }
}

app.Run();

// Helper method to convert PostgreSQL URI to Npgsql connection string
static string ConvertPostgresUrlToConnectionString(string postgresUrl)
{
    var uri = new Uri(postgresUrl);
    var userInfo = uri.UserInfo.Split(':');

    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = userInfo.Length > 0 ? userInfo[0] : "";
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
}

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
