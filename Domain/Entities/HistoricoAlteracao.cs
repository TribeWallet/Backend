namespace TribeWallet.Domain.Entities;

/// <summary>
/// Trilha de auditoria. Referencia a entidade alterada de forma genérica,
/// então serve para compromissos, pagamentos ou qualquer registro futuro.
/// </summary>
public class HistoricoAlteracao : EntidadeBase
{
    public Guid HistoricoAlteracaoId { get; set; } = Guid.NewGuid();

    /// <summary>Autor da alteração.</summary>
    public Guid UsuarioId { get; set; }

    public string Entidade { get; set; } = string.Empty;

    public Guid EntidadeId { get; set; }

    public TipoAlteracao Tipo { get; set; }

    /// <summary>Snapshot JSON antes da operação. Nulo em criações.</summary>
    public string? DadosAntes { get; set; }

    /// <summary>Snapshot JSON depois da operação. Nulo em exclusões.</summary>
    public string? DadosDepois { get; set; }

    public DateTime DataHora { get; set; }

    public Usuario? Usuario { get; set; }
}
