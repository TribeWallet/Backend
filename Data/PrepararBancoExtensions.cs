using Microsoft.EntityFrameworkCore;

namespace TribeWallet.Data;

/// <summary>Rotina de startup que prepara o banco antes da aplicação atender requisições.</summary>
public static class PrepararBancoExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes e executa o seed, conforme as flags do .env.
    /// Conveniente em desenvolvimento; em produção deixe APLICAR_MIGRATIONS_NO_STARTUP=false e
    /// rode `dotnet ef database update` como passo separado do deploy, para o schema não mudar
    /// sozinho quando uma instância nova sobe.
    /// </summary>
    public static async Task PrepararBancoAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var aplicarMigrations = configuration.GetValue("APLICAR_MIGRATIONS_NO_STARTUP", false);
        var executarSeed = configuration.GetValue("EXECUTAR_SEED", false);

        if (!aplicarMigrations && !executarSeed)
        {
            return;
        }

        using var escopo = services.CreateScope();
        var context = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Banco");

        if (aplicarMigrations)
        {
            var pendentes = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();

            if (pendentes.Length == 0)
            {
                logger.LogInformation("Nenhuma migration pendente.");
            }
            else
            {
                logger.LogInformation("Aplicando {Total} migration(s): {Migrations}", pendentes.Length, string.Join(", ", pendentes));
                await context.Database.MigrateAsync(cancellationToken);
            }
        }

        if (executarSeed)
        {
            await DatabaseSeeder.SemearAsync(
                context,
                configuration["SEED_SENHA_PADRAO"] ?? "Tribe@123",
                configuration["STORAGE_BASE_URL"] ?? "http://localhost:9000/tribewallet",
                logger,
                cancellationToken);
        }
    }
}
