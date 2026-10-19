namespace TribeWallet.Application;

public class UsuarioResponseDTO
{
    public string UsuarioToken { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string ImageUrl { get; set; }
    public DateTime? DeletedAt { get; set; }
}