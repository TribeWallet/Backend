using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Pagamento;

public interface IPagamentoRepository
{
    Task<Domain.Entities.Pagamento> Add(Domain.Entities.Pagamento pagamento);
    Task<Domain.Entities.Pagamento> Update(Domain.Entities.Pagamento pagamento);
    Task Delete(Domain.Entities.Pagamento pagamento);
    Task<Domain.Entities.Pagamento?> GetByToken(string token);
    Task<ICollection<Domain.Entities.Pagamento>> GetAllByIntegranteCompromissoToken(string integranteCompromissoToken, bool deleted = false);
    Task<IEnumerable<Domain.Entities.Pagamento>> GetAll(bool deleted = false);
}