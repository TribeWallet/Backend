namespace TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Domain.Entities;
public interface IIntegranteCompromissoRepository
{
    public Task<IntegranteCompromisso> GetByToken(string token);
    public Task<IntegranteCompromisso?> GetByIntegranteToken(string integranteToken, bool deleted = false);
    public Task<ICollection<IntegranteCompromisso>> GetAllByCompromissoToken(string compromissoToken, bool deleted = false);
    public Task<IntegranteCompromisso> Create(IntegranteCompromisso integranteCompromisso);
    public Task<ICollection<IntegranteCompromisso>> CreateMultiple(ICollection<IntegranteCompromisso> integranteCompromissos);
    public Task<IntegranteCompromisso> Update(IntegranteCompromisso integranteCompromisso);
    public Task Delete(IntegranteCompromisso integranteCompromisso);
}