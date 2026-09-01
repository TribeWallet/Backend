namespace TribeWallet.Application.Integrante;
using TribeWallet.Domain.Entities;

public interface IIntegranteRepository
{
    public Task<ICollection<Integrante>> GetAllByGrupoToken(string token);
}