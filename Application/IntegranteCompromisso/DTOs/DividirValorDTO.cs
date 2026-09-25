namespace TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Domain.Entities;
public class DividirValorDTO
{
    public required CompromissoFinanceiro CompromissoFinanceiro { get; set; }
    public ICollection<IntegranteCompromisso> Participacoes { get; set; } = [];
    public decimal ValorTotal { get; set; }
    
}