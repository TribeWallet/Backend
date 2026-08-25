using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using TribeWallet.Data;

// Antes do CreateBuilder: é aqui que ASPNETCORE_URLS e ASPNETCORE_ENVIRONMENT saem do .env
// e entram no processo, a tempo do host lê-los. TraversePath sobe os diretórios até achar
// o arquivo. NoClobber faz o ambiente real ter precedência sobre o .env.
Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Deixa as variáveis do .env acessíveis via IConfiguration.
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(ConnectionString.Montar(builder.Configuration))
    .UseSnakeCaseNamingConvention());

var app = builder.Build();

// Migrations e seed são controlados pelo .env, então dá para desligar os dois em produção.
await app.Services.PrepararBancoAsync(builder.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
