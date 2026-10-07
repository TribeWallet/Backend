using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.IntegranteCompromisso.DTOs;

public class IntegranteCompromissoDashResponseDTO
{
    public string IntegranteCompromissoToken { get; set; }
    public string NomeCompromisso { get; set; }
    public string ImagemCompromissoUrl { get; set; }
    public string NomeGrupo { get; set; }
    public decimal ValorDevedor { get; set; }
    public SituacaoPagamento SituacaoPagamento { get; set; }
    public DateTime DataVencimento { get; set; }
    public string Categoria { get; set; }
    public bool Vencido  { get; set; }
}