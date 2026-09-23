namespace TribeWallet.Application.IntegranteCompromisso;

public interface IIntegranteCompromissoRepository
{
    Task<Domain.Entities.IntegranteCompromisso?> GetByToken(string token);
}