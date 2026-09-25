using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class IntegranteCompromissoRepository: IIntegranteCompromissoRepository
{
    private readonly AppDbContext _dbContext;

    public IntegranteCompromissoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IntegranteCompromisso> GetByToken(string token)
    {
        var integranteCompromisso = await _dbContext.IntegrantesCompromissos.FirstOrDefaultAsync(ic => ic.Token == token);
        
        if (integranteCompromisso == null)
            throw new Exception();
        return integranteCompromisso;
    }

    public async Task<IntegranteCompromisso?> GetByIntegranteToken(string integranteToken, bool deleted = false)
    {
        IntegranteCompromisso integranteCompromisso;
        if (!deleted)
        {
            integranteCompromisso = await _dbContext.IntegrantesCompromissos
                .Where(ic => ic.Integrante.Token == integranteToken)
                .Where(ic => ic.DeletedAt == null)
                .FirstOrDefaultAsync();
        }
        else
        {
            integranteCompromisso = await _dbContext.IntegrantesCompromissos
                .Where(ic => ic.Integrante.Token == integranteToken)
                .FirstOrDefaultAsync();
        }
        
        return  integranteCompromisso;
    }

    public async Task<ICollection<IntegranteCompromisso>> GetAllByCompromissoToken(string compromissoToken, bool deleted = false)
    {
        ICollection<IntegranteCompromisso> integranteCompromissos;
        if (!deleted)
        {
            integranteCompromissos = await _dbContext.IntegrantesCompromissos
                .Where(ic => ic.Compromisso.Token == compromissoToken)
                .Where(ic => ic.DeletedAt == null)
                .Include(ic => ic.Integrante)
                .ToListAsync();
        }
        else
        {
            integranteCompromissos = await _dbContext.IntegrantesCompromissos
                .Where(ic => ic.Compromisso.Token == compromissoToken)
                .Include(ic => ic.Integrante)
                .ToListAsync();
        }
        
        return  integranteCompromissos;
    }

    public async Task<IntegranteCompromisso> Create(IntegranteCompromisso integranteCompromisso)
    {
        var newIntegranteCompromisso = _dbContext.IntegrantesCompromissos.Add(integranteCompromisso);
        await _dbContext.SaveChangesAsync();
        
        return newIntegranteCompromisso.Entity;
    }

    public async Task<ICollection<IntegranteCompromisso>> CreateMultiple(ICollection<IntegranteCompromisso> integranteCompromissos)
    {
        _dbContext.IntegrantesCompromissos.AddRange(integranteCompromissos);
        await _dbContext.SaveChangesAsync();

        return integranteCompromissos;
    }

    public async Task<IntegranteCompromisso> Update(IntegranteCompromisso integranteCompromisso)
    {
        var newIntegranteCompromisso = _dbContext.IntegrantesCompromissos.Update(integranteCompromisso);
        await _dbContext.SaveChangesAsync();
        
        return newIntegranteCompromisso.Entity;
    }

    public async Task Delete(IntegranteCompromisso integranteCompromisso)
    {
        if (integranteCompromisso.DeletedAt == null)
        {
            integranteCompromisso.DeletedAt = DateTime.UtcNow;
            _dbContext.IntegrantesCompromissos.Update(integranteCompromisso);
            await _dbContext.SaveChangesAsync();
        }
    }
}