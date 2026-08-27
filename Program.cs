using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using TribeWallet.Data;
using TribeWallet.Application.Usuario;
using TribeWallet.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;

// Antes do CreateBuilder: é aqui que ASPNETCORE_URLS e ASPNETCORE_ENVIRONMENT saem do .env
// e entram no processo, a tempo do host lê-los. TraversePath sobe os diretórios até achar
// o arquivo. NoClobber faz o ambiente real ter precedência sobre o .env.
Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Deixa as variáveis do .env acessíveis via IConfiguration.
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(ConnectionString.Montar(builder.Configuration))
    .UseSnakeCaseNamingConvention());

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure the HTTP request pipeline.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DbContext, AppDbContext>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositoryImplementation>();
builder.Services.AddScoped<UsuarioService>();

var app = builder.Build();

// Migrations e seed são controlados pelo .env, então dá para desligar os dois em produção.
await app.Services.PrepararBancoAsync(builder.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
/*app.UseAuthentication();
app.UseAuthorization();*/
app.MapControllers();

app.Run();