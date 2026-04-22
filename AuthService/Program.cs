using AspNet.Security.OAuth.GitHub;
using AuthService.Data;
using AuthService.Interface;
using AuthService.Middleware;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------- DB ----------
builder.Services.AddDbContext<AuthDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- DI ----------
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();

// ---------- Authentication ----------
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SpendSmart";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SpendSmart";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
})
// Cookie scheme is required by Google/GitHub handlers to temporarily persist the external identity.
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
{
    o.Cookie.Name = "SpendSmart.External";
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
{
    o.ClientId     = builder.Configuration["Authentication:Google:ClientId"]     ?? "google-client-id";
    o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "google-client-secret";
    o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.CallbackPath = "/signin-google"; // must match the URL registered in Google Cloud Console
    o.SaveTokens = true;
    o.Scope.Add("email");
    o.Scope.Add("profile");
})
.AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, o =>
{
    o.ClientId     = builder.Configuration["Authentication:GitHub:ClientId"]     ?? "github-client-id";
    o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "github-client-secret";
    o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.CallbackPath = "/signin-github";
    o.SaveTokens = true;
    o.Scope.Add("user:email");
});

builder.Services.AddAuthorization();

// ---------- Controllers + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SpendSmart AuthService", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token below (without 'Bearer ' prefix). Swagger will add it automatically.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply EF Core migrations automatically on startup (dev convenience).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.Run();