using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Pagamento.DTOs;

namespace TribeWallet.Application.IntegranteCompromisso.DTOs;

public class IntegranteCompromissoReturnDTO
{
    public string IntegranteCompromissoToken { get; set; }
    public IntegranteReturnDTO Integrante { get; set; }
    public CompromissoFinanceiroReturnDTO CompromissoFinanceiro { get; set; }
    public decimal ValorDevedor { get; set; }
    public decimal ValorPago { get; set; }
    public ICollection<PagamentoReturnDTO> Pagamentos { get; set; } = [];
}