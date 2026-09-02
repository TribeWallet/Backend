using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.Grupo;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class GrupoRepository : IGrupoRepository
{
    private readonly AppDbContext _dbContext;

    public GrupoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Grupo> GetByToken(string token)
    {
        var grupo = await _dbContext.Grupos.FirstOrDefaultAsync(u => u.Token == token);
        return grupo ?? throw new Exception("Grupo não encontrado pelo token informado");
    }

    public async Task<IEnumerable<Grupo>> GetAllByUsuarioToken(string usuarioToken)
    {
        var grupos = await _dbContext.Grupos
            .Include(g => g.Integrantes)
            .ThenInclude(i => i.Usuario)
            .Where(g => g.Integrantes.Any(i => i.Usuario.Token == usuarioToken)).ToListAsync();

        return grupos;
    }

    public async Task<Grupo> Create(Grupo grupo)
    {
        var newGrupo = _dbContext.Grupos.Add(grupo);
        await _dbContext.SaveChangesAsync();
        
        return newGrupo.Entity;
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