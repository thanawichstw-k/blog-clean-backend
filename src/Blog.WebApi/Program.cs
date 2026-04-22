using System.Text;
using Blog.Infrastructure;
using Blog.Infrastructure.Data;
using Blog.Infrastructure.Identity;
using Blog.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    var solutionRoot = FindSolutionRoot(app.Environment.ContentRootPath);
    var dbJsonPath = Path.Combine(solutionRoot, "db.json");

    System.Diagnostics.Debug.WriteLine($"[MYLOG] contentRoot = {app.Environment.ContentRootPath}");
    System.Diagnostics.Debug.WriteLine($"[MYLOG] solutionRoot = {solutionRoot}");
    System.Diagnostics.Debug.WriteLine($"[MYLOG] dbJsonPath = {dbJsonPath}");
    System.Diagnostics.Debug.WriteLine($"[MYLOG] dbJsonExists = {File.Exists(dbJsonPath)}");

    if (File.Exists(dbJsonPath))
    {
        await DbInitializer.InitializeAsync(app.Services, dbJsonPath);
    }
}

app.Run();