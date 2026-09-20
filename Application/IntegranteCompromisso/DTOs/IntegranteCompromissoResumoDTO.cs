using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Pagamento.DTOs;

namespace TribeWallet.Application.IntegranteCompromisso.DTOs;

public class IntegranteCompromissoResumoDTO
{
    public string IntegranteCompromissoToken { get; set; }
    public decimal ValorDevedor { get; set; }
    public decimal ValorPago { get; set; }
    public DateTime? DeletedAt { get; set; }
    public IntegranteResponseDTO Integrante { get; set; }
    public ICollection<PagamentoResponseDTO> Pagamentos { get; set; } = [];
}