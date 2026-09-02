using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Application.Relatorio.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Compromisso.DTOs;

public class CompromissoFinanceiroResponseDTO
{
    public string CompromissoFinanceiroToken { get; set; }
    public GrupoResponseDTO? Grupo { get; set; }
    public string Titulo { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime Data { get; set; }
    public TipoDivisao TipoDivisao { get; set; }
    public string? ImagemUrl { get; set; }
    public string Categoria { get; set; }
    public ICollection<IntegranteCompromissoResponseDTO> Participacoes { get; set; } = [];
    public ICollection<RelatorioResponseDTO>  Relatorios { get; set; } = [];
}