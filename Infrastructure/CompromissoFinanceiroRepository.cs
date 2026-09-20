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

    public async Task<CompromissoFinanceiro> GetByToken(string token)
    {
        var compromisso = await _dbContext.CompromissosFinanceiros
            .Where(c => c.Token == token)
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes)
            .ThenInclude(p => p.Integrante)
            .ThenInclude(i => i.Usuario)
            .FirstOrDefaultAsync();

        return compromisso;
    }

    public async Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken)
    {
        var compromissos = await _dbContext.CompromissosFinanceiros
            .Where(c => c.Participacoes
                .Any(p => p.Integrante.Token == integranteToken))
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes)
                .ThenInclude(p => p.Integrante)
                    .ThenInclude(i => i.Usuario)
            .ToListAsync();
        
        return compromissos;
    }

    public async Task<ICollection<CompromissoFinanceiro>> GetAllByGrupoToken(string grupoToken)
    {
        var compromissos = await _dbContext.CompromissosFinanceiros
            .Where(c => c.Grupo.Token == grupoToken)
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes)
            .ThenInclude(p => p.Integrante)
            .ThenInclude(i => i.Usuario)
            .ToListAsync();
        
        return compromissos;
    }
    
    public async Task<CompromissoFinanceiro> Create(CompromissoFinanceiro compromissoFinanceiro)
    {
        var compromisso = _dbContext.CompromissosFinanceiros.Add(compromissoFinanceiro);
        await _dbContext.SaveChangesAsync();

        return compromisso.Entity;
    }

    public async Task<CompromissoFinanceiro> Update(CompromissoFinanceiro compromissoFinanceiro)
    {
        var newCompromisso = _dbContext.CompromissosFinanceiros.Update(compromissoFinanceiro);
        await _dbContext.SaveChangesAsync();
        
        return newCompromisso.Entity;
    }
}