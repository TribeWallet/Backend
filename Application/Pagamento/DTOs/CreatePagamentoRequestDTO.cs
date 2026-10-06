using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Pagamento.DTOs;

public class CreatePagamentoRequestDTO
{
    public required string IntegranteCompromissoToken { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public IFormFile Comprovante { get; set; }
    public MetodoPagamento Metodo { get; set; } // Pode mapear para o Enum `MetodoPagamento` no Service
}