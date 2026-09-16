namespace TribeWallet.Application;

public class UsuarioResponseDTO
{
    public string UsuarioToken { get; set; }
    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }

    /// <summary>Preenchido quando o usuário foi excluído (soft delete). Quem consome decide o que mostrar.</summary>
    public DateTime? DeletedAt { get; set; }
}