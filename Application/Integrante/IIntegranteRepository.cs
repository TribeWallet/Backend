namespace TribeWallet.Application.Integrante;
using TribeWallet.Domain.Entities;

public interface IIntegranteRepository
{
    public Task<ICollection<Integrante>> GetAllByGrupoToken(string token, bool deleted);
    public Task<Integrante> GetByToken(string integranteToken);
    public Task<Integrante> Create(Integrante integrante);
    public Task<Integrante> Delete(Integrante integrante);
}