namespace TribeWallet.Domain.Entities;

/// <summary>
/// Aviso entre usuários. Entidade + EntidadeId apontam de forma genérica para o
/// registro que originou o evento (grupo, compromisso, pagamento...).
/// </summary>
public class Notificacao : EntidadeBase
{
    public Guid NotificacaoId { get; set; } = Guid.NewGuid();

    public Guid UsuarioDestinoId { get; set; }

    /// <summary>Nulo quando o aviso é gerado pelo sistema, sem um usuário por trás.</summary>
    public Guid? UsuarioOrigemId { get; set; }

    public string Entidade { get; set; } = string.Empty;

    public Guid EntidadeId { get; set; }

    public TipoNotificacao Tipo { get; set; }

    public string Mensagem { get; set; } = string.Empty;

    public DateTime DataEnvio { get; set; }

    public bool Lida { get; set; }

    public Usuario? UsuarioDestino { get; set; }

    public Usuario? UsuarioOrigem { get; set; }
}
