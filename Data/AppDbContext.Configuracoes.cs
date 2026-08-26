using Microsoft.EntityFrameworkCore;
using TribeWallet.Entities;

namespace TribeWallet.Data;

/// <summary>
/// Mapeamento das entidades. Um método por tabela, na mesma ordem das migrations.
/// Relacionamentos são declarados sempre no lado dependente (quem carrega a FK).
/// </summary>
public partial class AppDbContext
{
    private static void ConfigurarUsuarios(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(u => u.UsuarioId);
            e.Property(u => u.UsuarioId).ValueGeneratedNever();
            e.Property(u => u.Nome).HasMaxLength(100).IsRequired();
            e.Property(u => u.Sobrenome).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(254).IsRequired();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.Imagem).HasMaxLength(2048);
            e.Property(u => u.HashSenha).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

    private static void ConfigurarGrupos(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Grupo>(e =>
        {
            e.ToTable("grupos");
            e.HasKey(g => g.GrupoId);
            e.Property(g => g.GrupoId).ValueGeneratedNever();
            e.Property(g => g.Nome).HasMaxLength(120).IsRequired();
            e.Property(g => g.Descricao).HasMaxLength(500);
        });

    private static void ConfigurarIntegrantes(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Integrante>(e =>
        {
            e.ToTable("integrantes");
            e.HasKey(i => i.IntegranteId);
            e.Property(i => i.IntegranteId).ValueGeneratedNever();

            e.HasOne(i => i.Usuario)
                .WithMany(u => u.Integrantes)
                .HasForeignKey(i => i.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Grupo)
                .WithMany(g => g.Integrantes)
                .HasForeignKey(i => i.GrupoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Um usuário entra uma única vez em cada grupo.
            e.HasIndex(i => new { i.UsuarioId, i.GrupoId }).IsUnique();
        });

    private static void ConfigurarCompromissosFinanceiros(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CompromissoFinanceiro>(e =>
        {
            e.ToTable("compromissos_financeiros");
            e.HasKey(c => c.CompromissoFinanceiroId);
            e.Property(c => c.CompromissoFinanceiroId).ValueGeneratedNever();
            e.Property(c => c.Titulo).HasMaxLength(150).IsRequired();
            e.Property(c => c.ValorTotal).HasPrecision(14, 2).IsRequired();
            e.Property(c => c.Data).HasColumnType("timestamp with time zone").IsRequired();
            e.Property(c => c.TipoDivisao).HasConversion<int>().IsRequired();
            e.Property(c => c.Imagem).HasMaxLength(2048);
            e.Property(c => c.Categoria).HasMaxLength(80);

            e.HasOne(c => c.Grupo)
                .WithMany(g => g.Compromissos)
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(c => c.Data);
        });

    private static void ConfigurarIntegrantesCompromissos(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<IntegranteCompromisso>(e =>
        {
            e.ToTable("integrantes_compromissos");
            e.HasKey(ic => ic.IntegranteCompromissoId);
            e.Property(ic => ic.IntegranteCompromissoId).ValueGeneratedNever();
            e.Property(ic => ic.ValorDevedor).HasPrecision(14, 2).IsRequired();
            e.Property(ic => ic.ValorPago).HasPrecision(14, 2).IsRequired().HasDefaultValue(0m);

            e.HasOne(ic => ic.Integrante)
                .WithMany(i => i.Compromissos)
                .HasForeignKey(ic => ic.IntegranteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ic => ic.Compromisso)
                .WithMany(c => c.Participacoes)
                .HasForeignKey(ic => ic.CompromissoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cada integrante aparece uma vez por compromisso.
            e.HasIndex(ic => new { ic.IntegranteId, ic.CompromissoId }).IsUnique();
        });

    private static void ConfigurarPagamentos(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Pagamento>(e =>
        {
            e.ToTable("pagamentos");
            e.HasKey(p => p.PagamentoId);
            e.Property(p => p.PagamentoId).ValueGeneratedNever();
            e.Property(p => p.Valor).HasPrecision(14, 2).IsRequired();
            e.Property(p => p.Data).HasColumnType("timestamp with time zone").IsRequired();
            e.Property(p => p.ComprovanteUrl).HasMaxLength(2048);
            e.Property(p => p.Metodo).HasConversion<int>().IsRequired();

            e.HasOne(p => p.IntegranteCompromisso)
                .WithMany(ic => ic.Pagamentos)
                .HasForeignKey(p => p.IntegranteCompromissoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => p.Data);
        });

    private static void ConfigurarRelatorios(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Relatorio>(e =>
        {
            e.ToTable("relatorios");
            e.HasKey(r => r.RelatorioId);
            e.Property(r => r.RelatorioId).ValueGeneratedNever();
            e.Property(r => r.Tipo).HasConversion<int>().IsRequired();
            e.Property(r => r.DataHora).HasColumnType("timestamp with time zone").IsRequired();
            e.Property(r => r.ConteudoUrl).HasMaxLength(2048);

            e.HasOne(r => r.Usuario)
                .WithMany(u => u.Relatorios)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Compromisso)
                .WithMany(c => c.Relatorios)
                .HasForeignKey(r => r.CompromissoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    private static void ConfigurarNotificacoes(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Notificacao>(e =>
        {
            e.ToTable("notificacoes");
            e.HasKey(n => n.NotificacaoId);
            e.Property(n => n.NotificacaoId).ValueGeneratedNever();
            e.Property(n => n.Entidade).HasMaxLength(80).IsRequired();
            e.Property(n => n.Tipo).HasConversion<int>().IsRequired();
            e.Property(n => n.Mensagem).HasMaxLength(500).IsRequired();
            e.Property(n => n.DataEnvio).HasColumnType("timestamp with time zone").IsRequired();
            e.Property(n => n.Lida).HasDefaultValue(false);

            e.HasOne(n => n.UsuarioDestino)
                .WithMany(u => u.NotificacoesRecebidas)
                .HasForeignKey(n => n.UsuarioDestinoId)
                .OnDelete(DeleteBehavior.Cascade);

            // A origem some sem levar a notificação junto: avisos de sistema não têm remetente.
            e.HasOne(n => n.UsuarioOrigem)
                .WithMany(u => u.NotificacoesEnviadas)
                .HasForeignKey(n => n.UsuarioOrigemId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(n => new { n.UsuarioDestinoId, n.Lida });
            e.HasIndex(n => new { n.Entidade, n.EntidadeId });
        });

    private static void ConfigurarHistoricoAlteracoes(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<HistoricoAlteracao>(e =>
        {
            e.ToTable("historico_alteracoes");
            e.HasKey(h => h.HistoricoAlteracaoId);
            e.Property(h => h.HistoricoAlteracaoId).ValueGeneratedNever();
            e.Property(h => h.Entidade).HasMaxLength(80).IsRequired();
            e.Property(h => h.Tipo).HasConversion<int>().IsRequired();

            // jsonb deixa o Postgres consultar dentro do snapshot.
            e.Property(h => h.DadosAntes).HasColumnType("jsonb");
            e.Property(h => h.DadosDepois).HasColumnType("jsonb");
            e.Property(h => h.DataHora).HasColumnType("timestamp with time zone").IsRequired();

            // Restringe: apagar um usuário não pode apagar a trilha de auditoria dele.
            e.HasOne(h => h.Usuario)
                .WithMany(u => u.HistoricoAlteracoes)
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(h => new { h.Entidade, h.EntidadeId });
            e.HasIndex(h => h.DataHora);
        });
}
