namespace TribeWallet.Application.Grupo.DTOs;

public class ReturnGrupoDTO
{
    public string Token { get; set; }
    
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }
}