namespace TribeWallet.Application.ConviteGrupo.DTOs;

public class CreateConviteGrupoRequestDTO
{
    /// <summary>Opcional. Se informado, o convite é restrito a esse e-mail e o link é enviado a ele.</summary>
    public string? Email { get; set; }
}

public class ConviteGrupoResponseDTO
{
    public string ConviteToken { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string GrupoToken { get; set; } = string.Empty;
    public string? EmailConvidado { get; set; }
    public DateTime ExpiraEm { get; set; }
    public bool EmailEnviado { get; set; }
}

public class ConviteGrupoInfoDTO
{
    public string GrupoNome { get; set; } = string.Empty;
    public string? GrupoDescricao { get; set; }
    public string ConvidadoPor { get; set; } = string.Empty;
    public string? EmailConvidado { get; set; }
    public DateTime ExpiraEm { get; set; }
}
