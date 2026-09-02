using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Relatorio.DTOs;

public class RelatorioResponseDTO
{
    public string RelatorioToken { get; set; }
    public UsuarioResponseDTO Usuario { get; set; }
    public CompromissoFinanceiroResponseDTO CompromissoFinanceiro { get; set; }
    public TipoRelatorio Tipo { get; set; }
    public DateTime DataHora { get; set; }
    public string? ConteudoUrl { get; set; }
}