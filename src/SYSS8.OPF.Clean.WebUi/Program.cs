var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// UI-state och auth är scoped så varje användarsession får sin egen state.
// TokenStore är singleton eftersom token lagras i minnet globalt.
builder.Services
    .AddScoped<IUiStatus, UiStatus>()
    .AddSingleton<ITokenStore, TokenStore>()
    .AddScoped<AuthState>()
    .AddScoped<ApiClient>();

// DelegatingHandler registreras i DI så att HttpClientFactory kan skapa den.
builder.Services.AddTransient<JwtMessageHandler>();

// WebApi-basadress hämtas från config för att hålla miljöberoenden utanför kod.
var webApiBaseUrl = builder.Configuration["WebApi:BaseUrl"] ?? "https://localhost:5001";

// HttpClientFactory skapar klienten och lägger till JWT per anrop via handlern.
builder.Services.AddHttpClient("WebApi", c =>
{
    c.BaseAddress = new Uri(webApiBaseUrl);
})
.AddHttpMessageHandler<JwtMessageHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<SYSS8.OPF.Clean.WebUi.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
