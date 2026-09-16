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
        var grupo = await _dbContext.Grupos
            .Include(g => g.Integrantes)
            .FirstOrDefaultAsync(u => u.Token == token);
        return grupo ?? throw new Exception("Grupo não encontrado pelo token informado");
    }

    public async Task<IEnumerable<Grupo>> GetAllByUsuarioToken(string usuarioToken, bool deleted)
    {
        List<Grupo> grupos;
        
        // !deleted significa que ele vai buscar apenas registros ativos (deletedAt == null)
        if (!deleted)
        {
            grupos = await _dbContext.Grupos
                .Include(g => g.Integrantes)
                .ThenInclude(i => i.Usuario)
                .Where(g => g.Integrantes.Any(i => i.Usuario.Token == usuarioToken))
                .Where(g => g.DeletedAt == null)
                .ToListAsync();

            return grupos;
        }
        
        // busca registros ativos e inativos
        grupos = await _dbContext.Grupos
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

    public async Task<Grupo> Update(Grupo grupo)
    {
        var newGrupo = _dbContext.Grupos.Update(grupo);
        await _dbContext.SaveChangesAsync();
        
        return  newGrupo.Entity;
    }


    /// <summary>
    /// Soft delete: a linha continua no banco, só passa a carregar a data da exclusão. Repetir a
    /// chamada não mexe na data original, então o horário guardado é sempre o da primeira exclusão.
    /// </summary>
    public async Task<Grupo> Delete(Grupo grupo)
    {
        if (grupo.DeletedAt is null)
        {
            grupo.DeletedAt = DateTime.UtcNow;
            _dbContext.Grupos.Update(grupo);
            await _dbContext.SaveChangesAsync();
        }

        return grupo;
    }
}