using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class CompromissoFinanceiroRepository : ICompromissoFinanceiroRepository
{
    private readonly AppDbContext _dbContext;

    public CompromissoFinanceiroRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken)
    {
        var compromissos = await _dbContext.CompromissosFinanceiros.Include(c => c.Participacoes)
            .ThenInclude(p => p.Integrante)
            .Where(c => c.Participacoes.Any(p => p.Integrante.Token == integranteToken)).ToListAsync();
        
        return compromissos;
    }
}