namespace TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Domain.Entities;
public interface ICompromissoFinanceiroRepository
{
    public Task<CompromissoFinanceiro> Create(CompromissoFinanceiro compromissoFinanceiro);
    public Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken);
    public Task<ICollection<CompromissoFinanceiro>> GetAllByGrupoToken(string grupoToken);
}