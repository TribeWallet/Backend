namespace TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Domain.Entities;
public record DividirValorRecord(
    CompromissoFinanceiro CompromissoFinanceiro,
    ICollection<IntegranteCompromisso> Participacoes,
    decimal ValorTotal,
    TipoDivisao TipoDivisao
    );