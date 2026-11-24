using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Zepto.Backend.Data;
using Zepto.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=ZeptoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

builder.Services.AddDbContext<ZeptoDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Register Application Services
builder.Services.AddScoped<ITokenService, TokenService>();

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("Configure JWT_SECRET or JwtSettings:Secret before starting the API.");
if (Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException("The JWT signing secret must be at least 32 bytes long.");
}
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ZeptoApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "ZeptoClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                context.Fail("The token does not identify a user.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ZeptoDbContext>();
            var currentRole = await dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => user.Role)
                .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (currentRole == null || context.Principal?.Identity is not ClaimsIdentity identity)
            {
                context.Fail("The token user no longer exists.");
                return;
            }

            foreach (var roleClaim in identity.FindAll(ClaimTypes.Role).ToList())
            {
                identity.RemoveClaim(roleClaim);
            }

            identity.AddClaim(new Claim(ClaimTypes.Role, currentRole));
        }
    };
});

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// 5. Swagger Configuration with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SwiftBasket Store API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    var schemeRef = new OpenApiSecuritySchemeReference("Bearer");
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            schemeRef,
            new List<string>()
        }
    });
});

var app = builder.Build();

// Seed Database automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ZeptoDbContext>();
            DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the Zepto database.");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwiftBasket API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger at the app root
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
