using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Notificacoes;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly AppDbContext _dbContext;

    public NotificacaoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(IEnumerable<Notificacao> notificacoes)
    {
        _dbContext.Notificacoes.AddRange(notificacoes);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<Notificacao>> ListarPorUsuario(Guid usuarioId, int limite) =>
        _dbContext.Notificacoes
            .Include(n => n.UsuarioOrigem)
            .Where(n => n.UsuarioDestinoId == usuarioId && n.DeletedAt == null)
            .OrderByDescending(n => n.DataEnvio)
            .Take(limite)
            .ToListAsync();

    public Task<int> ContarNaoLidas(Guid usuarioId) =>
        _dbContext.Notificacoes
            .CountAsync(n => n.UsuarioDestinoId == usuarioId && n.DeletedAt == null && !n.Lida);

    public Task<Notificacao?> GetByToken(string token) =>
        _dbContext.Notificacoes.FirstOrDefaultAsync(n => n.Token == token && n.DeletedAt == null);

    public async Task Update(Notificacao notificacao)
    {
        _dbContext.Notificacoes.Update(notificacao);
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarcarTodasComoLidas(Guid usuarioId)
    {
        var pendentes = await _dbContext.Notificacoes
            .Where(n => n.UsuarioDestinoId == usuarioId && n.DeletedAt == null && !n.Lida)
            .ToListAsync();

        foreach (var notificacao in pendentes)
            notificacao.Lida = true;

        await _dbContext.SaveChangesAsync();
    }

    public Task<List<Domain.Entities.IntegranteCompromisso>> GetFatiasEmAberto(Guid usuarioId, DateTime ate) =>
        _dbContext.IntegrantesCompromissos
            .Include(ic => ic.Compromisso)
            .Where(ic => ic.Integrante!.UsuarioId == usuarioId
                         && ic.Integrante.DeletedAt == null
                         && ic.DeletedAt == null
                         && ic.Compromisso!.DeletedAt == null
                         && ic.ValorPago < ic.ValorDevedor
                         && ic.Compromisso.Data <= ate)
            .ToListAsync();

    public async Task<HashSet<Guid>> CobrancasJaEnviadas(Guid usuarioId, IEnumerable<Guid> compromissoIds, DateTime desde)
    {
        var ids = compromissoIds.ToList();
        var enviadas = await _dbContext.Notificacoes
            .Where(n => n.UsuarioDestinoId == usuarioId
                        && n.Tipo == TipoNotificacao.CobrancaPendente
                        && n.DataEnvio >= desde
                        && ids.Contains(n.EntidadeId))
            .Select(n => n.EntidadeId)
            .ToListAsync();

        return enviadas.ToHashSet();
    }

    public async Task<Dictionary<Guid, string>> ResolverTokens(IEnumerable<Notificacao> notificacoes)
    {
        var lista = notificacoes.ToList();
        var tokens = new Dictionary<Guid, string>();

        var grupos = lista.Where(n => n.Entidade == nameof(Grupo)).Select(n => n.EntidadeId).Distinct().ToList();
        if (grupos.Count > 0)
            foreach (var g in await _dbContext.Grupos.Where(g => grupos.Contains(g.GrupoId)).Select(g => new { g.GrupoId, g.Token }).ToListAsync())
                tokens[g.GrupoId] = g.Token;

        var compromissos = lista.Where(n => n.Entidade == nameof(CompromissoFinanceiro)).Select(n => n.EntidadeId).Distinct().ToList();
        if (compromissos.Count > 0)
            foreach (var c in await _dbContext.CompromissosFinanceiros.Where(c => compromissos.Contains(c.CompromissoFinanceiroId)).Select(c => new { c.CompromissoFinanceiroId, c.Token }).ToListAsync())
                tokens[c.CompromissoFinanceiroId] = c.Token;

        var pagamentos = lista.Where(n => n.Entidade == nameof(Pagamento)).Select(n => n.EntidadeId).Distinct().ToList();
        if (pagamentos.Count > 0)
            foreach (var p in await _dbContext.Pagamentos.Where(p => pagamentos.Contains(p.PagamentoId)).Select(p => new { p.PagamentoId, p.Token }).ToListAsync())
                tokens[p.PagamentoId] = p.Token;

        return tokens;
    }
}
