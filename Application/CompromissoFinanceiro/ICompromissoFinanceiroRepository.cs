namespace TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Domain.Entities;
public interface ICompromissoFinanceiroRepository
{
    public Task<CompromissoFinanceiro> GetByToken(string token, bool deleted = false);
    public Task<ICollection<CompromissoFinanceiro>> GetAllByIntegranteToken(string integranteToken, bool deleted = false);
    public Task<ICollection<CompromissoFinanceiro>> GetAllByGrupoToken(string grupoToken, bool deleted = false);
    public Task<CompromissoFinanceiro> Create(CompromissoFinanceiro compromissoFinanceiro);
    public Task<CompromissoFinanceiro> Update(CompromissoFinanceiro compromissoFinanceiro);
    public Task Delete(CompromissoFinanceiro compromissoFinanceiro);
}