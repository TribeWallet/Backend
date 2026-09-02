using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class UpdateGrupoRequestDTO
{
    public string Nome { get; set; }
    public string? Descricao { get; set; }
}