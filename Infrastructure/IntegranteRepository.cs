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

    public async Task<ICollection<Integrante>> GetAllByGrupoToken(string token, bool deleted)
    {
        List<Integrante> integrantes;
        // !deleted significa que ele vai buscar apenas registros ativos (deletedAt == null)
        if (!deleted)
        {
            integrantes = await _dbContext.Integrantes
                .Include(i => i.Grupo)
                .Include(i => i.Usuario)
                .Where(i => i.Grupo.Token == token)
                .Where(i => i.DeletedAt == null)
                .ToListAsync();

            return integrantes;
        }
        
        // busca registros ativos e inativos
        integrantes = await _dbContext.Integrantes
            .Include(i => i.Grupo)
            .Include(i => i.Usuario)
            .Where(i => i.Grupo.Token == token)
            .ToListAsync();
        
        return integrantes;
    }

    public async Task<Integrante> GetByToken(string integranteToken)
    {
        var integrante = await _dbContext.Integrantes
            .Include(i => i.Grupo)
            .FirstOrDefaultAsync(i => i.Token == integranteToken);
        return integrante;
    }

    public async Task<Integrante> Create(Integrante integrante)
    {
        var newIntegrante = _dbContext.Integrantes.Add(integrante);
        await _dbContext.SaveChangesAsync();
        return newIntegrante.Entity;
    }

    public async Task<Integrante> Delete(Integrante integrante)
    {
        if (integrante.DeletedAt is null)
        {
            integrante.DeletedAt = DateTime.UtcNow;
            _dbContext.Integrantes.Update(integrante);
            await _dbContext.SaveChangesAsync();
        }

        return integrante;
    }
}