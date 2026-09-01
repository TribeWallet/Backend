using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class GrupoReturnDTO
{
    public string GrupoToken { get; set; }
    
    public string Nome { get; set; }

    public string? Descricao { get; set; }
    public ICollection<IntegranteReturnDTO> Integrantes { get; set; } = [];
    public ICollection<CompromissoFinanceiroReturnDTO> Compromissos { get; set; } = [];
}