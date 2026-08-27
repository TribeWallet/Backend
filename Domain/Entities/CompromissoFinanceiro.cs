namespace TribeWallet.Domain.Entities;

/// <summary>Uma despesa lançada no grupo e rateada entre os integrantes.</summary>
public class CompromissoFinanceiro : EntidadeBase
{
    public Guid CompromissoFinanceiroId { get; set; } = Guid.NewGuid();

    public Guid GrupoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    /// <summary>decimal, não float: dinheiro não cabe em ponto flutuante binário.</summary>
    public decimal ValorTotal { get; set; }

    public DateTime Data { get; set; }

    public TipoDivisao TipoDivisao { get; set; }

    /// <summary>URL da nota fiscal no servidor externo de arquivos.</summary>
    public string? Imagem { get; set; }

    public string? Categoria { get; set; }

    public Grupo? Grupo { get; set; }

    public ICollection<IntegranteCompromisso> Participacoes { get; set; } = [];

    public ICollection<Relatorio> Relatorios { get; set; } = [];
}
