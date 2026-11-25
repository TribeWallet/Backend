namespace TribeWallet.Domain.Entities;

public class Usuario : EntidadeBase
{
    public Guid UsuarioId { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;

    public string Sobrenome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Imagem { get; set; }

    public string HashSenha { get; set; } = string.Empty;

    public string? ResetSenhaTokenHash { get; set; }

    public DateTime? ResetSenhaExpiracao { get; set; }

    public ICollection<Integrante> Integrantes { get; set; } = [];

    public ICollection<Relatorio> Relatorios { get; set; } = [];

    public ICollection<Notificacao> NotificacoesRecebidas { get; set; } = [];

    public ICollection<Notificacao> NotificacoesEnviadas { get; set; } = [];

    public ICollection<HistoricoAlteracao> HistoricoAlteracoes { get; set; } = [];
}
