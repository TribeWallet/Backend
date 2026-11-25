namespace TribeWallet.Application;

public class ResetSenhaRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}