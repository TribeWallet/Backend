namespace TribeWallet.Domain;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Imagem { get; set; }
    public string HashSenha { get; set; }
}