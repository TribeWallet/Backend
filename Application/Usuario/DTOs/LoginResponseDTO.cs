namespace TribeWallet.Application;

public class LoginResponseDTO
{
    public UsuarioResponseDTO UsuarioResponseDto { get; set; }
    public string JwtToken { get; set; }
}