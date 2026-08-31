namespace TribeWallet.Application;

public class EditUsuarioDTO
{
    public Guid UsuarioId { get; set; }
    public string UuarioToken { get; set; }
    public string? Nome { get; set; }
    public string? Sobrenome { get; set; }
    public string? Username { get; set; }
    public string? Imagem { get; set; }
    public string? Senha { get; set; }
    public string JwtToken { get; set; }
}