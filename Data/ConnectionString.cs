using Microsoft.Extensions.Configuration;

namespace TribeWallet.Data;

/// <summary>Monta a connection string do Postgres a partir do que veio do .env.</summary>
public static class ConnectionString
{
    public static string Montar(IConfiguration configuration)
    {
        // Uma connection string completa, quando informada, vence a composição por partes.
        var completa = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(completa))
        {
            return completa;
        }

        var host = configuration["POSTGRES_HOST"] ?? "localhost";
        var porta = configuration["POSTGRES_PORT"] ?? "5432";
        var banco = configuration["POSTGRES_DB"] ?? "tribewallet";
        var usuario = configuration["POSTGRES_USER"] ?? "tribewallet";
        var senha = configuration["POSTGRES_PASSWORD"] ?? "tribewallet";

        return $"Host={host};Port={porta};Database={banco};Username={usuario};Password={senha}";
    }
}
