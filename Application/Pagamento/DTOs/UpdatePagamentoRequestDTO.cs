namespace TribeWallet.Application.Pagamento.DTOs;

public class UpdatePagamentoRequestDTO
{
    public decimal? Valor { get; set; }
    public DateTime? Data { get; set; }
    public string? ComprovanteBase64 { get; set; }
    public int? Metodo { get; set; } 
}