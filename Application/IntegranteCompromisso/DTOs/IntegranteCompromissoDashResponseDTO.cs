using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.IntegranteCompromisso.DTOs;

public class IntegranteCompromissoDashResponseDTO
{
    public string IntegranteCompromissoToken { get; set; }
    public string NomeCompromisso { get; set; }
    public string ImagemCompromissoUrl { get; set; }
    public string NomeGrupo { get; set; }
    public string Categoria { get; set; }
    public decimal ValorDevedor { get; set; }
    public decimal ValorPago { get; set; }
    public SituacaoPagamento SituacaoPagamento { get; set; }
    public DateTime DataVencimento { get; set; }
    public int DiasParaVencimento { get; set; }
    public bool Vencido  { get; set; }
}