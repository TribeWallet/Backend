namespace TribeWallet.Application.ConviteGrupo;
using TribeWallet.Domain.Entities;

public interface IConviteGrupoRepository
{
    Task<ConviteGrupo> Create(ConviteGrupo convite);
    Task<ConviteGrupo?> GetByToken(string token);
    Task<Integrante?> GetIntegrante(Guid grupoId, Guid usuarioId);
    Task RestaurarIntegrante(Integrante integrante);
}
