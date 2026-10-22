using TribeWallet.Application.Integrante;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Compromisso.DTOs;

public class CompromissoFinanceiroResponseMainDTO
{
    public string CompromissoFinanceiroToken { get; set; }
    public string GrupoToken { get; set; }
    public string Titulo { get; set; }
    public string? Imagem { get; set; }
    public SituacaoPagamento SituacaoPagamento { get; set; }
    public DateTime DataVencimento { get; set; }
    public int DiasParaVencimento { get; set; }
    public bool Vencido { get; set; }
    public TipoDivisao TipoDivisao { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPagoTotal { get; set; }
    public decimal ValorRestanteTotal { get; set; }
    public float ProgressoPagamentos { get; set; }
    public string Categoria { get; set; }
    public string NomeGrupo { get; set; }
    public int IntegrantesQuitadosCount { get; set; }
    public int IntegrantesDevendoCount { get; set; }
    public int IntegrantesTotalCount { get; set; }
    public ICollection<IntegranteResponseFromCompromissoDTO> Integrantes { get; set; }
    public DateTime? DeletedAt { get; set; }
}