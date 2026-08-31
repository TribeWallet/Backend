namespace TribeWallet.Application;

public class RegisterUsuarioDTO
{
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? Imagem { get; set; }
    public string Senha { get; set; }
}