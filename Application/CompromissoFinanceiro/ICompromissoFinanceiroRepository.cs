namespace TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Domain.Entities;
public interface ICompromissoFinanceiroRepository
{
    public Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken);
}