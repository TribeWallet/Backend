using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class GrupoResponseMainDTO
{
    public string GrupoToken { get; set; }
    
    public string Nome { get; set; }

    public string? Descricao { get; set; }

    public int IntegrantesCount { get; set; }
    public int CompromissosCount { get; set; }
    public decimal TotalEmAberto { get; set; }
    public decimal TotalPago { get; set; }
    public ICollection<IntegranteResumoDTO> Integrantes { get; set; }
    public ICollection<CompromissoFinanceiroResumoDTO> Compromissos { get; set; }
}