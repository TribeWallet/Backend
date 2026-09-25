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

    public async Task<CompromissoFinanceiro> GetByToken(string token, bool deleted = false)
    {
        var compromisso =  _dbContext.CompromissosFinanceiros
            .Where(c => c.Token == token)
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes)
            .ThenInclude(p => p.Integrante)
            .ThenInclude(i => i.Usuario);

        if (!deleted)
            compromisso
                .Where(c => c.DeletedAt == null);
        return await compromisso.FirstOrDefaultAsync();
    }

    public async Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken, bool deleted = false)
    {
        var compromissos = _dbContext.CompromissosFinanceiros
            .Where(c => c.Participacoes
                .Any(p => p.Integrante.Token == integranteToken))
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes)
            .ThenInclude(p => p.Integrante)
            .ThenInclude(i => i.Usuario);

        if (!deleted)
            compromissos.Where(c => c.DeletedAt == null);
        
        return await compromissos.ToListAsync();
    }

    public async Task<ICollection<CompromissoFinanceiro>> GetAllByGrupoToken(string grupoToken, bool deleted = false)
    {
        var compromissos = new List<CompromissoFinanceiro>();

        if (!deleted)
        {
            compromissos = await _dbContext.CompromissosFinanceiros
            .Where(c => c.Grupo.Token == grupoToken)
            .Include(c => c.Grupo)
            .Include(c => c.Participacoes
                .Where(p => p.DeletedAt == null))
            .ThenInclude(p => p.Integrante)
            .ThenInclude(i => i.Usuario)
            .Where(c => c.DeletedAt == null).ToListAsync();
        }
        else
        {
            compromissos = await _dbContext.CompromissosFinanceiros
                .Where(c => c.Grupo.Token == grupoToken)
                .Include(c => c.Grupo)
                .Include(c => c.Participacoes)
                .ThenInclude(p => p.Integrante)
                .ThenInclude(i => i.Usuario).ToListAsync();
        }
        
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

    public async Task Delete(CompromissoFinanceiro compromissoFinanceiro)
    {
        if (compromissoFinanceiro.DeletedAt == null)
        {
            compromissoFinanceiro.DeletedAt = DateTime.UtcNow;
            _dbContext.CompromissosFinanceiros.Update(compromissoFinanceiro);
            await  _dbContext.SaveChangesAsync();
        }
    }
}