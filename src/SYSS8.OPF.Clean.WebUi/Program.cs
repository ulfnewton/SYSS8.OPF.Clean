var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IUiStatus, UiStatus>();
builder.Services.AddSingleton<ITokenStore, TokenStore>();
builder.Services.AddTransient<JwtMessageHandler>();

builder.Services.AddHttpClient("WebApi", c =>
{
    c.BaseAddress = new Uri("https://localhost:5001");
})
.AddHttpMessageHandler<JwtMessageHandler>();

builder.Services
    .AddScoped<ApiClient>()
    .AddScoped<AuthState>();

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