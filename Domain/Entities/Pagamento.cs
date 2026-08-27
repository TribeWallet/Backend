namespace TribeWallet.Domain.Entities;

public class Pagamento : EntidadeBase
{
    public Guid PagamentoId { get; set; } = Guid.NewGuid();

    public Guid IntegranteCompromissoId { get; set; }

    public decimal Valor { get; set; }

    public DateTime Data { get; set; }

    /// <summary>URL do comprovante no servidor externo de arquivos.</summary>
    public string? ComprovanteUrl { get; set; }

    public MetodoPagamento Metodo { get; set; }

    public IntegranteCompromisso? IntegranteCompromisso { get; set; }
}
