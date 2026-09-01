using TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Pagamento.DTOs;

public class PagamentoReturnDTO
{
    public string PagamentoToken { get; set; }
    public IntegranteCompromissoReturnDTO IntegranteCompromisso { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string ComprovanteUrl { get; set; }
    public MetodoPagamento Metodo { get; set; }
}