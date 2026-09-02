using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Integrante;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class IntegranteRepository : IIntegranteRepository
{
    private readonly AppDbContext _dbContext;

    public IntegranteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<Integrante>> GetAllByGrupoToken(string token)
    {
        var integrantes = await _dbContext.Integrantes
            .Include(i => i.Grupo)
            .Where(i => i.Grupo.Token == token).ToListAsync();
        
        return integrantes;
    }

    public async Task<Integrante> Create(Integrante integrante)
    {
        var newIntegrante = _dbContext.Integrantes.Add(integrante);
        await _dbContext.SaveChangesAsync();
        return newIntegrante.Entity;
    }
}