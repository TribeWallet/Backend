using Microsoft.EntityFrameworkCore;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Data;

/// <summary>Sessão com o banco do TribeWallet. Os mapeamentos ficam em AppDbContext.Configuracoes.cs.</summary>
public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Pessoas cadastradas no sistema.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Grupos que dividem despesas.</summary>
    public DbSet<Grupo> Grupos => Set<Grupo>();

    /// <summary>Vínculo usuário ↔ grupo.</summary>
    public DbSet<Integrante> Integrantes => Set<Integrante>();

    /// <summary>Despesas lançadas nos grupos.</summary>
    public DbSet<CompromissoFinanceiro> CompromissosFinanceiros => Set<CompromissoFinanceiro>();

    /// <summary>Fatia de cada integrante em um compromisso.</summary>
    public DbSet<IntegranteCompromisso> IntegrantesCompromissos => Set<IntegranteCompromisso>();

    /// <summary>Quitações registradas, com comprovante.</summary>
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    /// <summary>Relatórios gerados pelos usuários.</summary>
    public DbSet<Relatorio> Relatorios => Set<Relatorio>();

    /// <summary>Avisos enviados entre usuários.</summary>
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    /// <summary>Trilha de auditoria das alterações.</summary>
    public DbSet<HistoricoAlteracao> HistoricoAlteracoes => Set<HistoricoAlteracao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarUsuarios(modelBuilder);
        ConfigurarGrupos(modelBuilder);
        ConfigurarIntegrantes(modelBuilder);
        ConfigurarCompromissosFinanceiros(modelBuilder);
        ConfigurarIntegrantesCompromissos(modelBuilder);
        ConfigurarPagamentos(modelBuilder);
        ConfigurarRelatorios(modelBuilder);
        ConfigurarNotificacoes(modelBuilder);
        ConfigurarHistoricoAlteracoes(modelBuilder);

        ConfigurarTokens(modelBuilder);
        ConfigurarAuditoria(modelBuilder);
    }

    /// <summary>
    /// Aplica o mapeamento do token público a toda entidade que herda de <see cref="EntidadeBase"/>,
    /// para nenhuma tabela ficar de fora por esquecimento. Roda depois das configurações
    /// individuais, então já enxerga todas as entidades do modelo.
    /// </summary>
    private static void ConfigurarTokens(ModelBuilder modelBuilder)
    {
        var entidades = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var entidade in entidades)
        {
            modelBuilder.Entity(entidade.ClrType, e =>
            {
                e.Property(nameof(EntidadeBase.Token))
                    .HasColumnType($"character({TokenExterno.Tamanho})")
                    .IsRequired();

                e.HasIndex(nameof(EntidadeBase.Token)).IsUnique();
            });
        }
    }

    /// <summary>
    /// Mapeia os carimbos de tempo de <see cref="EntidadeBase"/> em todas as tabelas, pelo mesmo
    /// motivo do token: nenhuma entidade fica de fora por esquecimento. O índice em
    /// <c>deleted_at</c> serve às consultas que separam ativos de excluídos.
    /// </summary>
    private static void ConfigurarAuditoria(ModelBuilder modelBuilder)
    {
        var entidades = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
            .ToList();

        foreach (var entidade in entidades)
        {
            modelBuilder.Entity(entidade.ClrType, e =>
            {
                // O default de now() é rede de segurança para insert fora do EF e é o que preenche
                // as linhas que já existiam quando a coluna foi criada; no caminho normal quem
                // manda é o AtribuirTimestamps.
                e.Property(nameof(EntidadeBase.CreatedAt))
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()")
                    .IsRequired();

                e.Property(nameof(EntidadeBase.UpdatedAt))
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()")
                    .IsRequired();

                // Nulo é o estado normal: só quem foi excluído carrega data aqui.
                e.Property(nameof(EntidadeBase.DeletedAt))
                    .HasColumnType("timestamp with time zone");

                e.HasIndex(nameof(EntidadeBase.DeletedAt));
            });
        }
    }

    /// <summary>
    /// Carimba <c>created_at</c> no insert e <c>updated_at</c> em toda alteração, usando um único
    /// instante por SaveChanges para as linhas gravadas juntas não diferirem por milissegundos.
    /// UTC porque as colunas são <c>timestamp with time zone</c>.
    /// </summary>
    private void AtribuirTimestamps()
    {
        var agora = DateTime.UtcNow;

        foreach (var entrada in ChangeTracker.Entries<EntidadeBase>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Entity.CreatedAt = agora;
                    entrada.Entity.UpdatedAt = agora;
                    break;

                case EntityState.Modified:
                    // CreatedAt não muda nem quando a entidade chega desanexada, com o valor zerado.
                    entrada.Property(nameof(EntidadeBase.CreatedAt)).IsModified = false;
                    entrada.Entity.UpdatedAt = agora;
                    break;
            }
        }
    }

    /// <summary>
    /// Sorteia o token das entidades novas no momento do insert. Fazer isso aqui — e não num
    /// inicializador de propriedade — evita gastar um SHA-256 por linha lida do banco.
    /// </summary>
    private void AtribuirTokens()
    {
        foreach (var entrada in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entrada.State == EntityState.Added && string.IsNullOrEmpty(entrada.Entity.Token))
            {
                entrada.Entity.Token = TokenExterno.Gerar();
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AtribuirTokens();
        AtribuirTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AtribuirTokens();
        AtribuirTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
