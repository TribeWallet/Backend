using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Grupo;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class GrupoRepositoryImplementation : IGrupoRepository
{
    private readonly AppDbContext _dbContext;

    public GrupoRepositoryImplementation(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Grupo> GetByToken(string token)
    {
        var grupo = await _dbContext.Grupos.FirstOrDefaultAsync(u => u.Token == token);
        return grupo ?? throw new Exception("Grupo não encontrado pelo token informado");
    }

    public async Task<IEnumerable<Grupo>> GetByUsuarioToken(string usuarioToken)
    {
        var grupos = await _dbContext.Grupos
            .Include(g => g.Integrantes)
            .ThenInclude(i => i.Usuario)
            .Where(g => g.Integrantes.Any(i => i.Usuario.Token == usuarioToken)).ToListAsync();
        
        return grupos;
    }

    public Task<Grupo> Create(Grupo grupo)
    {
        throw new NotImplementedException();
    }

    public Task<Grupo> Update(Grupo grupo)
    {
        throw new NotImplementedException();
    }


    public void Delete(int id)
    {
        throw new NotImplementedException();
    }
}