namespace TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Domain.Entities;
public interface IIntegranteCompromissoRepository
{
    public Task<IntegranteCompromisso> Create(IntegranteCompromisso integranteCompromisso);
}