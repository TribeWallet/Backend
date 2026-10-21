using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Integrante;

public class IntegranteResponseFromCompromissoDTO
{
    public string IntegranteToken  { get; set; }
    public string UsuarioToken { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string ImagemUrl { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorDevedor { get; set; }
    public decimal ValorRestante { get; set; }
    public Dictionary<MetodoPagamento, decimal> SomaPagamentosPorMetodo { get; set; }
    public bool Quitado { get; set; }
    public DateTime? DeletedAt { get; set; }
}