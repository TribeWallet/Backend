namespace TribeWallet.Application;

public class CreateUsuarioRequestDTO
{
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public IFormFile Imagem { get; set; }
    public string Senha { get; set; }
}