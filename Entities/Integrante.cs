namespace TribeWallet.Entities;

/// <summary>Vínculo entre um usuário e um grupo. É o integrante que assume dívidas.</summary>
public class Integrante : EntidadeBase
{
    public Guid IntegranteId { get; set; } = Guid.NewGuid();

    public Guid UsuarioId { get; set; }

    public Guid GrupoId { get; set; }

    public Usuario? Usuario { get; set; }

    public Grupo? Grupo { get; set; }

    public ICollection<IntegranteCompromisso> Compromissos { get; set; } = [];
}
