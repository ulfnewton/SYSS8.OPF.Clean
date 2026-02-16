using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Auth;
using SYSS8.OPF.Clean.WebApi.Endpoints;
using SYSS8.OPF.Clean.WebApi.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// DESIGN-VAL: InMemoryDatabase håller setupen enkel och snabb för undervisning.
builder.Services.AddDbContext<AuthorDbContext>(options => options.UseInMemoryDatabase("authors"));

builder.Services
    // INFO: IdentityCore räcker för API-scenarion och ger ett mer avskalat upplägg för kursen.
    .AddIdentityCore<User>(o =>
    {
        // TIPS: Mjukare lösenordsregler gör demo-konton lättare att prova i klassrum.
        o.User.RequireUniqueEmail = true;
        o.Password.RequireDigit = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredLength = 6;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AuthorDbContext>();

// INFO: JWT används i kursen för att tydligt visa stateless AuthN/AuthZ i ett API.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // OBS: Validering av issuer/audience/signing key visar helheten i JWT-säkerhet.
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
    options.AddPolicy("CanCreateAuthor", p => p.RequireRole("Admin", "Lärare"));
    options.AddPolicy("CanDeleteBook", p => p.RequireRole("Admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Urls.Add("https://localhost:5001");
app.UseHttpsRedirection();
// DESIGN-VAL: AuthN måste köras före AuthZ för att policies ska kunna läsa claims.
app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.MapAuth();
// INFO: Seeding i startup gör att demo-konton finns direkt när appen startas.
await IdentitySeeder.SeedAsync(app.Services);
app.Run();