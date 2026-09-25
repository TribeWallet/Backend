namespace TribeWallet.Application.Pagamento.DTOs;

public class CreatePagamentoRequestDTO
{
    public required string IntegranteCompromissoToken { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string? ComprovanteBase64 { get; set; } // Referente a US-019 (Anexar comprovantes)
    public int Metodo { get; set; } // Pode mapear para o Enum `MetodoPagamento` no Service
}