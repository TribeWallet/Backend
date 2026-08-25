namespace TribeWallet.Entities;

public class Grupo : EntidadeBase
{
    public Guid GrupoId { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public ICollection<Integrante> Integrantes { get; set; } = [];

    public ICollection<CompromissoFinanceiro> Compromissos { get; set; } = [];
}
