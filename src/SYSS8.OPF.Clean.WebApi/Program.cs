using Microsoft.EntityFrameworkCore;

using SYSS8.OPF.Clean.Infrastructure;
using SYSS8.OPF.Clean.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthorDbContext>(
    options => options.UseInMemoryDatabase("authors")
    );

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Urls.Add("https://localhost:5001");

app.UseHttpsRedirection();

app.MapEndpoints();
app.Run();
