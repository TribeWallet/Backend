using TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Application.Pagamento.DTOs;

namespace TribeWallet.Application;

public class DashboardResponseDTO
{
    public string UsuarioToken { get; set; }
    public int GruposAtivosCount { get; set; }
    public int PagamentosPendentesCount { get; set; }
    public int CompromissosAVencerCount { get; set; }
    public int CompromissosVencidosCount { get; set; }
    public float TaxaDePontualidade { get; set; }
    public ICollection<IntegranteCompromissoDashResponseDTO> TransacoesEVencimentos { get; set; }
}