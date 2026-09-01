using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.IntegranteCompromisso.DTOs;

namespace TribeWallet.Application.Integrante;

public class IntegranteReturnDTO
{
    public string IntegranteToken  { get; set; }
    public UsuarioReturnDTO Usuario { get; set; }
    public GrupoReturnDTO Grupo { get; set; }
    public ICollection<IntegranteCompromissoReturnDTO> Compromissos { get; set; } = [];
}