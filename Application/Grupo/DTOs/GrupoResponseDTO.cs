using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class GrupoResponseDTO
{
    public string GrupoToken { get; set; }
    
    public string Nome { get; set; }

    public string? Descricao { get; set; }

    /// <summary>Preenchido quando o grupo foi excluído (soft delete). Quem consome decide o que mostrar.</summary>
    public DateTime? DeletedAt { get; set; }
    public ICollection<IntegranteResponseDTO> Integrantes { get; set; } = [];
    public ICollection<CompromissoFinanceiroResponseDTO> Compromissos { get; set; } = [];
}