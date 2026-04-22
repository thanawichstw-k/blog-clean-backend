using System.Text;
using Blog.Infrastructure;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Identity;
using Blog.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

static string FindSolutionRoot(string startDirectory)
{
    var directory = new DirectoryInfo(startDirectory);

    while (directory is not null)
    {
        var hasSolution = directory.GetFiles("*.sln").Any();
        var hasDbJson = File.Exists(Path.Combine(directory.FullName, "db.json"));

        if (hasSolution || hasDbJson)
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Cannot find solution root that contains .sln or db.json");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    var host = builder.Configuration["ConnectionStrings:Host"];
    var port = builder.Configuration["ConnectionStrings:Port"];
    var database = builder.Configuration["ConnectionStrings:Database"];
    var username = builder.Configuration["ConnectionStrings:Username"];
    var password = builder.Configuration["ConnectionStrings:Password"];

    string connectionString;

    if (!string.IsNullOrWhiteSpace(host) &&
        !string.IsNullOrWhiteSpace(port) &&
        !string.IsNullOrWhiteSpace(database) &&
        !string.IsNullOrWhiteSpace(username) &&
        !string.IsNullOrWhiteSpace(password))
    {
        connectionString =
            $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        var defaultConnection = builder.Configuration.GetConnectionString("Default");

        if (builder.Environment.IsProduction())
        {
            throw new InvalidOperationException("Production database environment variables are missing.");
        }

        connectionString = defaultConnection
            ?? throw new InvalidOperationException("Default connection string is missing.");
    }

    options.UseNpgsql(connectionString);
});

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<BlogDbContext>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");

            if (exceptionFeature?.Error is not null)
            {
                logger.LogError(exceptionFeature.Error, "Unhandled exception for request {Path}", context.Request.Path);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Internal server error"
            });
        });
    });
}

app.MapGet("/", () => Results.Ok(new { message = "API is running" }));

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    var solutionRoot = FindSolutionRoot(app.Environment.ContentRootPath);
    var dbJsonPath = Path.Combine(solutionRoot, "db.json");

    if (File.Exists(dbJsonPath))
    {
        await DbInitializer.InitializeAsync(app.Services, dbJsonPath);
    }
}

app.Run();