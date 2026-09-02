using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Grupo.DTOs;

public class CreateGrupoRequestDTO
{
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public ICollection<CreateIntegranteRequestDTO> Integrantes { get; set; } = [];
}