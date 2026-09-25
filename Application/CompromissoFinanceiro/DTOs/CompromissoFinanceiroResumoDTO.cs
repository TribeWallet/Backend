using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.Integrante;

namespace TribeWallet.Application.Compromisso.DTOs;

public class CompromissoFinanceiroResumoDTO
{
    public string CompromissoFinanceiroToken { get; set; }
    public string Titulo { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime Data { get; set; }
    public GrupoResponseDTO? Grupo { get; set; }
    public string Categoria { get; set; }
}