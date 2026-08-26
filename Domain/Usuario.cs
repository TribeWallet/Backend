namespace TribeWallet.Domain;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? Imagem { get; set; }
    public string HashSenha { get; set; }

    public Usuario(
        string nome,
        string sobrenome,
        string email,
        string username,
        string hashSenha)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        Username = username;
        HashSenha = hashSenha;
    }

    public bool Authenticate(string email, string senha)
    {
        return email == Email && senha == HashSenha;
    }
    
    public void SetImagem(string imagem)
    {
        Imagem = imagem;
    }
}