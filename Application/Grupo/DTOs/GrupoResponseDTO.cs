using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class GrupoResponseDTO
{
    public string GrupoToken { get; set; }
    
    public string Nome { get; set; }

    public string? Descricao { get; set; }
    public ICollection<IntegranteResponseDTO> Integrantes { get; set; } = [];
    public ICollection<CompromissoFinanceiroResponseDTO> Compromissos { get; set; } = [];
}