var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register global UI status service (one source of truth)
builder.Services
    .AddScoped<IUiStatus, UiStatus>()
    .AddScoped<JwtMessageHandler>();

builder.Services.AddHttpClient("WebApi", c =>
{
    c.BaseAddress = new Uri("https://localhost:5001"); // matchar WebApi
})
.AddHttpMessageHandler<JwtMessageHandler>();

builder.Services
    .AddScoped<ApiClient>()
    .AddScoped<AuthState>()
    .AddScoped<ITokenStore, TokenStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<SYSS8.OPF.Clean.WebUi.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
