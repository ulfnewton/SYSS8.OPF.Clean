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

// Identity
// Design-val: Konfigurerar IdentityCore med User och Role, och ställer in lösenordspolicyn för att vara mer
// tillåtande (inga krav på siffror, versaler, specialtecken, och en kortare minsta längd) för att
// underlätta testning och utveckling. I produktion bör du ha en starkare policy.
builder.Services
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

// JWT
// Här konfigurerar vi så att JwtOptions kan bindas från appsettings.json, vilket gör det enkelt att ändra
// JWT-inställningarna utan att behöva ändra koden. Vi hämtar nyckeln från konfigurationen, konverterar den
// till bytes och ställer in TokenValidationParameters för att validera issuer, audience och signing key.
// Detta gör att vi kan använda JWT för att autentisera användare i vårt API.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

// Authentication
// Här konfigurerar vi JWT-baserad autentisering. Vi ställer in RequireHttpsMetadata till false för att underlätta
// utveckling (i produktion bör detta vara true), och SaveToken till true så att det genererade tokenet kan
// sparas i HttpContext. TokenValidationParameters definierar hur inkommande JWT-token ska valideras, inklusive
// att kontrollera issuer, audience och signing key mot de värden vi har i konfigurationen.
// - RequireHttpsMetadata har till uppgift att ange om JWT-bearer middleware ska kräva att metadata (t.ex. token)
//   skickas över HTTPS. 
// - SaveToken anger om det genererade tokenet ska sparas i HttpContext efter att användaren har autentiserats
//   (vilket används framför allt i middleware och kan vara användbart för senare användning i applikationen).
//   När SaveToken är satt till true, kommer det genererade JWT-tokenet att vara tillgängligt via
//   HttpContext.GetTokenAsync("access_token") eller liknande metoder, vilket kan vara användbart om du behöver
//   använda tokenet i andra delar av applikationen, t.ex. för att göra anrop till andra API:er som kräver autentisering.
// - TokenValidationParameters är en klass som används för att specificera hur JWT-token ska valideras när de tas emot
//   av servern. Här ställer vi in följande egenskaper:
//   - ValidateIssuer: Om true, kommer servern att kontrollera att issuer (utfärdare) av tokenet matchar det som
//     vi har definierat i ValidIssuer.
//   - ValidateAudience: Om true, kommer servern att kontrollera att audience (mål
//     för tokenet) matchar det som vi har definierat i ValidAudience.
//   - ValidateIssuerSigningKey: Om true, kommer servern att kontrollera att token
//     har signerats med den signing key som vi har definierat i IssuerSigningKey.
//   - ValidateLifetime: Om true, kontrolleras också att token inte har gått ut (exp) eller inte är giltigt ännu (nbf).
//     Detta minskar risken att för gamla tokens accepteras.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // RÄTT: I produktion ska detta vara true så att allt flödar över HTTPS.
        options.RequireHttpsMetadata = builder.Environment.IsDevelopment() ? false : true;

        // TIPS: Behövs främst om du vill plocka ut och vidareanvända token i andra delar av pipeline.
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            // Tydlighet (claims i tokenen matchar våra Authorize-attribut/policys)
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });

// Authorization
// Här definierar vi våra auktoriseringspolicys. Genom att använda AddPolicy kan vi skapa named policies
// som vi sedan kan referera till i våra endpoints för att kräva specifika roller eller krav. Detta
// centraliserar behörighetsdefinitionerna och gör det lättare att hantera och ändra dem på ett ställe.
// I det här fallet har vi definierat fyra policys: "CanCreateAuthor", "CanDeleteAuthor", "CanCreateBook"
// och "CanDeleteBook". Varje policy kräver att användaren har en specifik roll (eller roller) för att få
// åtkomst till de endpoints som kräver den policyn.
builder.Services.AddAuthorization(options =>
{
    // FIX: Lägger till de saknade policys som används i MapEndpointsExtensions.cs, för att centralisera
    // behörighetsdefinitionerna och undvika förvirring. Nu kräver "CanCreateAuthor" att användaren har
    // antingen "Admin" eller "Lärare" rollen, medan "CanDeleteAuthor" kräver "Admin" rollen. Samma gäller
    // för "CanCreateBook" och "CanDeleteBook".
    options.AddPolicy("CanCreateAuthor", policy => policy.RequireRole("Admin", "Lärare"));
    options.AddPolicy("CanDeleteAuthor", policy => policy.RequireRole("Admin"));   // ← saknades
    options.AddPolicy("CanCreateBook", policy => policy.RequireRole("Admin", "Lärare")); // ← saknades
    options.AddPolicy("CanDeleteBook", policy => policy.RequireRole("Admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS-policyn registreras här för att den ska kunna användas i app.UseCors("DevCors") längre ner i
// pipeline:n. Denna policy är väldigt öppen (AllowAnyOrigin, AllowAnyHeader, AllowAnyMethod) vilket är
// okej för utveckling men bör begränsas i produktion, t.ex. genom att använda
// WithOrigins("https://localhost:<ui-port>") för att endast tillåta anrop från den lokala
// frontend-utvecklingsservern.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p =>
        p.AllowAnyOrigin()     // DEV: enkelt; kan låsas till WithOrigins("https://localhost:<ui-port>")
         .AllowAnyHeader()
         .AllowAnyMethod());
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();
// Seed data
// Här anropar vi IdentitySeeder.SeedAsync för att seeda vår databas med initiala användare och roller. Detta
// är viktigt för att vi ska ha några användare att testa med när vi kör applikationen. Genom att använda
// app.Services kan vi få tillgång till den DI-container som har konfigurerats,
// och därmed få tillgång till de tjänster som IdentitySeeder behöver för att skapa användare och roller.
// FIX: Kör endast seeding i Development för att undvika överskrivning i andra miljöer.
if (app.Environment.IsDevelopment())
{
    await IdentitySeeder.SeedAsync(app.Services, CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// RÄTT: Låt launchSettings.json styra URL:er i dev för att undvika dubbelkonfiguration.
// (Tidigare rad app.Urls.Add("https://localhost:5001") är borttagen för tydlighet.)
//app.Urls.Add("https://localhost:5001");

app.UseHttpsRedirection();

// CORS måste registreras innan Authentication och Authorization i pipeline:n, så att CORS headers
// skickas även för 401/403-responser, vilket är viktigt för att frontend ska kunna hantera
// autentiserings- och auktoriseringsfel korrekt. Om CORS registreras efter Authentication/Authorization, så
// kommer inte CORS headers att inkluderas i 401/403-responser, vilket kan leda till att frontend inte
// kan tolka dessa fel korrekt och därmed inte visa rätt felmeddelanden eller vidta lämpliga åtgärder
// (t.ex. omdirigera till inloggningssidan).
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

app.UseAuthentication();
app.UseAuthorization();

// FIX: Flyttade MapAuthEndpoints till AuthenticationEndpoints-klassen för att centrera Auth-endpoints.
// (Se AuthenticationEndpoints.MapAuthEndpoints nedan – vi anropar den här via app.MapAuthEndpoints().)
app.MapEndpoints();

app.Run();