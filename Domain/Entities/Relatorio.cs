namespace TribeWallet.Domain.Entities;

/// <summary>O PDF vive no servidor externo de arquivos; aqui fica só a URL.</summary>
public class Relatorio : EntidadeBase
{
    public Guid RelatorioId { get; set; } = Guid.NewGuid();

    public Guid UsuarioId { get; set; }

    public Guid CompromissoId { get; set; }

    public TipoRelatorio Tipo { get; set; }

    public DateTime DataHora { get; set; }

    public string? ConteudoUrl { get; set; }

    public Usuario? Usuario { get; set; }

    public CompromissoFinanceiro? Compromisso { get; set; }
}
