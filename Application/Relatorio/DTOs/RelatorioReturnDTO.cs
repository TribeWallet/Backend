using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Relatorio.DTOs;

public class RelatorioReturnDTO
{
    public string RelatorioToken { get; set; }
    public UsuarioReturnDTO Usuario { get; set; }
    public CompromissoFinanceiroReturnDTO CompromissoFinanceiro { get; set; }
    public TipoRelatorio Tipo { get; set; }
    public DateTime DataHora { get; set; }
    public string? ConteudoUrl { get; set; }
}