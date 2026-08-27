namespace TribeWallet.Domain.Entities;

/// <summary>Fatia da despesa atribuída a um integrante: quanto deve e quanto já pagou.</summary>
public class IntegranteCompromisso : EntidadeBase
{
    public Guid IntegranteCompromissoId { get; set; } = Guid.NewGuid();

    public Guid IntegranteId { get; set; }

    public Guid CompromissoId { get; set; }

    public decimal ValorDevedor { get; set; }

    /// <summary>Total já quitado. Espelha a soma dos pagamentos.</summary>
    public decimal ValorPago { get; set; }

    public Integrante? Integrante { get; set; }

    public CompromissoFinanceiro? Compromisso { get; set; }

    public ICollection<Pagamento> Pagamentos { get; set; } = [];
}
