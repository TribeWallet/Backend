namespace TribeWallet.Domain.Entities;

/// <summary>
/// Convite para entrar num grupo por link. O <see cref="EntidadeBase.Token"/> é o código que vai
/// no link. Quando <see cref="EmailConvidado"/> está preenchido, só o usuário com esse e-mail
/// consegue aceitar; sem e-mail, qualquer usuário logado com o link entra até o convite expirar.
/// </summary>
public class ConviteGrupo : EntidadeBase
{
    public Guid ConviteGrupoId { get; set; } = Guid.NewGuid();

    public Guid GrupoId { get; set; }

    /// <summary>Integrante que gerou o convite.</summary>
    public Guid CriadoPorUsuarioId { get; set; }

    public string? EmailConvidado { get; set; }

    public DateTime ExpiraEm { get; set; }

    public Grupo? Grupo { get; set; }

    public Usuario? CriadoPorUsuario { get; set; }
}
