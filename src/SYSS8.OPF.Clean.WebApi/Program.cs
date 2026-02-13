using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.Infrastructure.Identity;
using SYSS8.OPF.Clean.WebApi.Auth;
using SYSS8.OPF.Clean.WebApi.Endpoints;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthorDbContext>(
    options => options.UseInMemoryDatabase("authors")
);
builder.Services
    .AddIdentityCore<User>(
        options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        }
    )
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AuthorDbContext>();

// JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new ArgumentNullException("Jwt:Key is missing in appsettings.json");
var keyBytes = Encoding.UTF8.GetBytes(key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreateAuthor", policy => policy.RequireRole("Admin", "Lärare"));
    options.AddPolicy("CanDeleteBook", policy => policy.RequireRole("Admin"));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p =>
        p.AllowAnyOrigin()     // DEV: enkelt; kan låsas till WithOrigins("https://localhost:<ui-port>")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

await IdentitySeeder.SeedAsync(app.Services, CancellationToken.None);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Urls.Add("https://localhost:5001");

app.UseHttpsRedirection();
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.Run();
