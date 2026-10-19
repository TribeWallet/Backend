namespace TribeWallet.Application.Pagamento.DTOs;

public class UpdatePagamentoRequestDTO
{
    public decimal? Valor { get; set; }
    public DateTime? Data { get; set; }
    public IFormFile Comprovante { get; set; }
    public int? Metodo { get; set; } 
}