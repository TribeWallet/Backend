namespace TribeWallet.Domain.Entities;

/// <summary>
/// Guarda metadados dos arquivos enviados
/// </summary>
public class Arquivo : EntidadeBase
{ 
    public Guid ArquivoId { get; set; } = Guid.NewGuid();
    public string Nome  { get; set; }
    public TipoConteudo Tipo { get; set; }
    public string Path { get; set; }
}