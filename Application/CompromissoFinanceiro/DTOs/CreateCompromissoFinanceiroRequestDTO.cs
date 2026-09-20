using TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Compromisso.DTOs;

public class CreateCompromissoFinanceiroRequestDTO
{
    public string Titulo { get; set; }
    public decimal ValorToral { get; set; }
    public DateTime Data { get; set; }
    public TipoDivisao TipoDivisao { get; set; }
    public string? Imagem  { get; set; }
    public string Categoria { get; set; }
    public ICollection<CreateIntegranteCompromissoRequestDTO>? Participacoes { get; set; }
}