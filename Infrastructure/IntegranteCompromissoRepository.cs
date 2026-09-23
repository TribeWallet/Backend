using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class IntegranteCompromissoRepository : IIntegranteCompromissoRepository
{
    private readonly AppDbContext _dbContext;

    public IntegranteCompromissoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IntegranteCompromisso?> GetByToken(string token)
    {
        return await _dbContext.IntegrantesCompromissos
            .FirstOrDefaultAsync(ic => ic.Token == token);
    }
}