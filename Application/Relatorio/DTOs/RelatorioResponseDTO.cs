using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Relatorio.DTOs;

public class RelatorioResponseDTO
{
    public TipoRelatorio Tipo { get; set; }
    public DateTime DataHora { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalDevido { get; set; }
    public ICollection<CategoriasCompromissosRelatorioResponseDTO> TotaisPagosPorCategorias { get; set; }
    public ICollection<MetodosRelatorioResponseDTO> TotaisPagosPorMetodos { get; set; }
    public ICollection<CompromissoEmAbertoRelatorioResponseDTO> CompromissosEmAberto { get; set; }
}

public class RelatorioResponseByUsuartioDTO : RelatorioResponseDTO
{
    public string NomeCompleto { get; set; }
    public ICollection<UserGruposRelatorioResponseDTO> Grupos { get; set; }
}

public class RelatorioResponseByGrupoDTO : RelatorioResponseDTO
{
    public string Nome { get; set; }
}

public class CategoriasCompromissosRelatorioResponseDTO
{
    public string Categoria { get; set; }
    public decimal ValorPagoTotal { get; set; }
}

public class MetodosRelatorioResponseDTO
{
    public MetodoPagamento MetodoPagamento { get; set; }
    public decimal TotalPago { get; set; }
}

public class CompromissoEmAbertoRelatorioResponseDTO
{
    public string Nome { get; set; }
    public DateTime DataVencimento { get; set; }
    public decimal ValorEmAberto { get; set; }
}
public class UserGruposRelatorioResponseDTO
{
    public string NomeGrupo { get; set; }
    public decimal TotalPago { get; set; }
}