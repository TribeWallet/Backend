using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Pagamento;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class PagamentoRepository : IPagamentoRepository
{
    private readonly AppDbContext _dbContext;

    public PagamentoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Pagamento> Add(Pagamento pagamento)
    {
        await _dbContext.Pagamentos.AddAsync(pagamento);
        await _dbContext.SaveChangesAsync();
        return pagamento;
    }

    public async Task<Pagamento> Update(Pagamento pagamento)
    {
        _dbContext.Pagamentos.Update(pagamento);
        await _dbContext.SaveChangesAsync();
        return pagamento;
    }

    public async Task Delete(Pagamento pagamento)
    {
        pagamento.DeletedAt = DateTime.UtcNow;
        _dbContext.Pagamentos.Update(pagamento);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Pagamento?> GetByToken(string token)
    {
        return await _dbContext.Pagamentos
            .Include(p => p.IntegranteCompromisso)
                .ThenInclude(ic => ic.Integrante)
                    .ThenInclude(i => i.Usuario)
            .Include(p => p.IntegranteCompromisso)
                .ThenInclude(ic => ic.Compromisso)
                    .ThenInclude(c => c.Grupo) // Assumindo que Compromisso tem relação com Grupo
            .FirstOrDefaultAsync(p => p.Token == token);
    }

    public async Task<IEnumerable<Pagamento>> GetAll(bool deleted = false)
    {
        var query = _dbContext.Pagamentos
            .Include(p => p.IntegranteCompromisso)
                .ThenInclude(ic => ic.Integrante)
                    .ThenInclude(i => i.Usuario)
            .Include(p => p.IntegranteCompromisso)
                .ThenInclude(ic => ic.Compromisso)
                    .ThenInclude(c => c.Grupo)
            .AsQueryable();

        if (!deleted)
            query = query.Where(p => p.DeletedAt == null);

        return await query.ToListAsync();
    }
}