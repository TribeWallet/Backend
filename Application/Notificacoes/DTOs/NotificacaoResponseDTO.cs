using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Notificacoes.DTOs;

public class NotificacaoResponseDTO
{
    public string NotificacaoToken { get; set; } = string.Empty;

    public TipoNotificacao Tipo { get; set; }

    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Tipo do registro que originou o aviso: Grupo, CompromissoFinanceiro ou Pagamento.</summary>
    public string Entidade { get; set; } = string.Empty;

    /// <summary>Token público do registro de origem; nulo se ele não existir mais.</summary>
    public string? EntidadeToken { get; set; }

    /// <summary>Nome de quem gerou o aviso; nulo quando veio do sistema.</summary>
    public string? OrigemNome { get; set; }

    public DateTime DataEnvio { get; set; }

    public bool Lida { get; set; }
}

public class NotificacaoContagemDTO
{
    public int NaoLidas { get; set; }
}
