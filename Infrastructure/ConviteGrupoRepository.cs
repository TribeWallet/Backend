using Microsoft.EntityFrameworkCore;
using TribeWallet.Application.ConviteGrupo;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class ConviteGrupoRepository : IConviteGrupoRepository
{
    private readonly AppDbContext _dbContext;

    public ConviteGrupoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConviteGrupo> Create(ConviteGrupo convite)
    {
        var entry = _dbContext.ConvitesGrupo.Add(convite);
        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }

    public Task<ConviteGrupo?> GetByToken(string token) =>
        _dbContext.ConvitesGrupo
            .Include(c => c.Grupo)
            .Include(c => c.CriadoPorUsuario)
            .FirstOrDefaultAsync(c => c.Token == token && c.DeletedAt == null);

    public Task<Integrante?> GetIntegrante(Guid grupoId, Guid usuarioId) =>
        _dbContext.Integrantes
            .Include(i => i.Usuario)
            .Include(i => i.Grupo)
            .FirstOrDefaultAsync(i => i.GrupoId == grupoId && i.UsuarioId == usuarioId);

    public async Task RestaurarIntegrante(Integrante integrante)
    {
        integrante.DeletedAt = null;
        _dbContext.Integrantes.Update(integrante);
        await _dbContext.SaveChangesAsync();
    }
}
