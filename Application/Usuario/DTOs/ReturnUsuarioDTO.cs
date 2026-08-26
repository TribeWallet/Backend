namespace TribeWallet.Application;

public class ReturnUsuarioDTO
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }

    public ReturnUsuarioDTO(int usuarioId, string nome, string sobrenome, string email, string username)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        Username = username;
    }
}