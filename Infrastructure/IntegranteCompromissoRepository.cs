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

    public async Task<IntegranteCompromisso> Create(IntegranteCompromisso integranteCompromisso)
    {
        var newIntegranteCompromisso = _dbContext.IntegrantesCompromissos.Add(integranteCompromisso);
        await _dbContext.SaveChangesAsync();
        
        return newIntegranteCompromisso.Entity;
    }
}