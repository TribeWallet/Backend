namespace TribeWallet.Application.IntegranteCompromisso.DTOs;

public class CreateIntegranteCompromissoRequestDTO
{
    public string IntegranteToken { get; set; }
    public decimal ValorDevedor { get; set; }
    public decimal ValorPago { get; set; } = 0;
}