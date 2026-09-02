using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.IntegranteCompromisso.DTOs;

namespace TribeWallet.Application.Integrante;

public class IntegranteResponseDTO
{
    public string IntegranteToken  { get; set; }
    public UsuarioResponseDTO Usuario { get; set; }
    public string GrupoToken { get; set; }
    public ICollection<IntegranteCompromissoResponseDTO> Compromissos { get; set; } = [];
}