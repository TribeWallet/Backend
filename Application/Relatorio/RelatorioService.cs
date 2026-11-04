using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Relatorio.DTOs;
using TribeWallet.Application.Usuario;

namespace TribeWallet.Application.Relatorio;
using TribeWallet.Domain.Entities;

public class RelatorioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly ICompromissoFinanceiroRepository _compromissoFinanceiroRepository;
    private readonly IIntegranteRepository _integranteRepository;
    private readonly UsuarioService _usuarioService;
    private readonly GrupoService _grupoService;
    private readonly CompromissoFinanceiroService _compromissoFinanceiroService;
    
    public RelatorioService(IUsuarioRepository usuarioRepository, IGrupoRepository grupoRepository, ICompromissoFinanceiroRepository compromissoFinanceiroRepository, UsuarioService usuarioService, GrupoService grupoService, CompromissoFinanceiroService compromissoFinanceiroService, IIntegranteRepository integranteRepository)
    {
        _usuarioRepository = usuarioRepository;
        _grupoRepository = grupoRepository;
        _compromissoFinanceiroRepository = compromissoFinanceiroRepository;
        _usuarioService = usuarioService;
        _grupoService = grupoService;
        _compromissoFinanceiroService = compromissoFinanceiroService;
        _integranteRepository = integranteRepository;
    }
    
    public async Task<RelatorioResponseDTO> GetByUsuarioToken(string usuarioToken)
    {
        var dadosUsuario = await _usuarioRepository.GetByToken(usuarioToken);
        var dadosCompromissos = await _compromissoFinanceiroService.GetAllByUsuarioToken(usuarioToken, deleted: false);
        
        var response = PrepareDataForUsuario(dadosCompromissos, dadosUsuario);
        return response;
    }

    public async Task<byte[]> GenUsuarioPdf(string usuarioToken)
    {
        var dadosUsuario = await _usuarioRepository.GetByToken(usuarioToken);
        var dadosCompromissos = await _compromissoFinanceiroService.GetAllByUsuarioToken(usuarioToken, deleted: false);
        
        var mockDtoUsuario = new RelatorioResponseByUsuartioDTO
        {
            Tipo = TipoRelatorio.ExtratoIndividual,
            DataHora = DateTime.Now,
            NomeCompleto = "Carlos Eduardo Silva",
            TotalPago = 3450.50m,
            TotalDevido = 620.00m,
            
            Grupos = new List<UserGruposRelatorioResponseDTO>
            {
                new UserGruposRelatorioResponseDTO { NomeGrupo = "República", TotalPago = 2100.00m },
                new UserGruposRelatorioResponseDTO { NomeGrupo = "Viagem para a Chapada", TotalPago = 1350.50m }
            },

            TotaisPagosPorCategorias = new List<CategoriasCompromissosRelatorioResponseDTO>
            {
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Aluguél", ValorPagoTotal = 1800.00m },
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Compras", ValorPagoTotal = 850.50m },
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Lazer", ValorPagoTotal = 800.00m }
            },

            TotaisPagosPorMetodos = new List<MetodosRelatorioResponseDTO>
            {
                new MetodosRelatorioResponseDTO { MetodoPagamento = MetodoPagamento.Pix, TotalPago = 2800.00m },
                new MetodosRelatorioResponseDTO { MetodoPagamento = MetodoPagamento.CartaoCredito, TotalPago = 650.50m }
            },

            CompromissosEmAberto = new List<CompromissoEmAbertoRelatorioResponseDTO>
            {
                new CompromissoEmAbertoRelatorioResponseDTO 
                { 
                    Nome = "Conta de Internet", 
                    DataVencimento = DateTime.Now.AddDays(5), 
                    ValorEmAberto = 120.00m 
                },
                new CompromissoEmAbertoRelatorioResponseDTO 
                { 
                    Nome = "Conta de Energia", 
                    DataVencimento = DateTime.Now.AddDays(12), 
                    ValorEmAberto = 500.00m 
                }
            }
        };

        var mockDtoGrupo = new RelatorioResponseByGrupoDTO
        {
            Tipo = TipoRelatorio.ResumoGrupo,
            DataHora = DateTime.Now,
            NomeGrupo = "Despesas da República",
            TotalPago = 5890.00m,
            TotalDevido = 1250.75m,
            
            TotaisPagosPorCategorias = new List<CategoriasCompromissosRelatorioResponseDTO>
            {
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Aluguel e Condomínio", ValorPagoTotal = 3200.00m },
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Supermercado", ValorPagoTotal = 1690.00m },
                new CategoriasCompromissosRelatorioResponseDTO { Categoria = "Streaming e Internet", ValorPagoTotal = 1000.00m }
            },

            TotaisPagosPorMetodos = new List<MetodosRelatorioResponseDTO>
            {
                new MetodosRelatorioResponseDTO { MetodoPagamento = MetodoPagamento.Pix, TotalPago = 4500.00m },
                new MetodosRelatorioResponseDTO { MetodoPagamento = MetodoPagamento.CartaoCredito, TotalPago = 1390.00m }
            },

            CompromissosEmAberto = new List<CompromissoEmAbertoRelatorioResponseDTO>
            {
                new CompromissoEmAbertoRelatorioResponseDTO 
                { 
                    Nome = "Conta de Luz - Outubro", 
                    DataVencimento = DateTime.Now.AddDays(3), 
                    ValorEmAberto = 450.50m 
                },
                new CompromissoEmAbertoRelatorioResponseDTO 
                { 
                    Nome = "Manutenção do Portão", 
                    DataVencimento = DateTime.Now.AddDays(10), 
                    ValorEmAberto = 800.25m 
                }
            }
        };

        var dataForPdf = PrepareDataForUsuario(dadosCompromissos, dadosUsuario);
        var reponseData = GeneratePdf(dataForPdf);
        return reponseData;
    }

    public async Task<RelatorioResponseDTO> GetByIntegranteToken(string usuarioToken, string grupoToken)
    {
        var dadosUsuario = await _usuarioRepository.GetByToken(usuarioToken);
        var dadosCompromissos = await _compromissoFinanceiroService.GetAllByUsuarioToken(dadosUsuario.Token, deleted: false);
        
        var response = PrepareDataForUsuario(dadosCompromissos, dadosUsuario, grupoToken:grupoToken);
        return response;
    }

    public async Task<byte[]> GenGrupoPdf(string usuarioToken, string grupoToken)
    {
        var dadosUsuario = await _usuarioRepository.GetByToken(usuarioToken);
        var dadosCompromissos = await _compromissoFinanceiroService.GetAllByUsuarioToken(dadosUsuario.Token, deleted: false);
        
        var dataForPdf = PrepareDataForUsuario(dadosCompromissos, dadosUsuario, grupoToken:grupoToken);
        var reponseData = GeneratePdf(dataForPdf);
        return reponseData;
    }
private RelatorioResponseDTO PrepareDataForUsuario(
    List<CompromissoFinanceiroResponseMainDTO> compromissosData, 
    Usuario usuarioData, 
    string? grupoToken = null)
{
    var dados = GetCompromissosData(compromissosData, usuarioData.Token, grupoToken);

    if (!string.IsNullOrEmpty(grupoToken))
    {
        return new RelatorioResponseByGrupoDTO
        {
            NomeUsuario = $"{usuarioData.Nome} { usuarioData.Sobrenome}",
            NomeGrupo = dados.nomeGrupo,
            Tipo = TipoRelatorio.ExtratoIndividual,
            DataHora = DateTime.Now,
            TotalPago = dados.totalPago,
            TotalDevido = dados.totalDevido,
            TotaisPagosPorCategorias = dados.totaisPagosPorCategoria,
            TotaisPagosPorMetodos = dados.totaisPagosPorMetodo,
            CompromissosEmAberto = dados.compromissosEmAberto
        };
    }

    return new RelatorioResponseByUsuartioDTO
    {
        NomeCompleto = $"{usuarioData.Nome} {usuarioData.Sobrenome}",
        Tipo = TipoRelatorio.ExtratoIndividual,
        DataHora = DateTime.Now,
        TotalPago = dados.totalPago,
        TotalDevido = dados.totalDevido,
        Grupos = dados.grupos,
        TotaisPagosPorCategorias = dados.totaisPagosPorCategoria,
        TotaisPagosPorMetodos = dados.totaisPagosPorMetodo,
        CompromissosEmAberto = dados.compromissosEmAberto
    };
}

private (decimal totalPago, decimal totalDevido, 
    string nomeGrupo,
    ICollection<UserGruposRelatorioResponseDTO> grupos,
    List<CompromissoEmAbertoRelatorioResponseDTO> compromissosEmAberto,
    List<CategoriasCompromissosRelatorioResponseDTO> totaisPagosPorCategoria, 
    List<MetodosRelatorioResponseDTO> totaisPagosPorMetodo
    ) GetCompromissosData(
        List<CompromissoFinanceiroResponseMainDTO> compromissosData, 
        string usuarioToken, 
        string? grupoToken = null)
{
    // Se foi passado um grupoToken, filtra a lista para aquele grupo
    if (!string.IsNullOrEmpty(grupoToken))
    {
        compromissosData = compromissosData
            .Where(c => c.GrupoToken == grupoToken)
            .ToList();
    }

    decimal totalPagoResponse = 0;
    decimal totalDevidoResponse = 0;
    string nomeGrupoResponse = string.Empty;
    var gruposResponse = new List<UserGruposRelatorioResponseDTO>();
    var compromissosEmAbertoResponse = new List<CompromissoEmAbertoRelatorioResponseDTO>();

    if (compromissosData.Any() && !string.IsNullOrEmpty(grupoToken))
    {
        nomeGrupoResponse = compromissosData.First().NomeGrupo ?? string.Empty;
    }

    if (string.IsNullOrEmpty(grupoToken))
    {
        gruposResponse = compromissosData
            .SelectMany(c => c.Integrantes, (c, p) => new { GrupoToken = c.GrupoToken, NomeGrupo = c.NomeGrupo, Participacao = p })
            .Where(x => x.Participacao.UsuarioToken == usuarioToken)
            .GroupBy(x => new { x.GrupoToken, x.NomeGrupo })
            .Select(g => new UserGruposRelatorioResponseDTO
            {
                GrupoToken = g.Key.GrupoToken,
                NomeGrupo = g.Key.NomeGrupo,
                TotalPago = g.Sum(x => x.Participacao.ValorPago)
            })
            .ToList();
    }

    var totaisPagosPorCategoriaResponse = compromissosData
        .SelectMany(c => c.Integrantes, (c, p) => new { c.Categoria, Participacao = p })
        .Where(x => x.Participacao.UsuarioToken == usuarioToken)
        .GroupBy(x => x.Categoria)
        .Select(g => new CategoriasCompromissosRelatorioResponseDTO
        {
            Categoria = g.Key,
            ValorPagoTotal = g.Sum(x => x.Participacao.ValorPago)
        })
        .ToList();

    var totaisPagosPorMetodoResponse = compromissosData
        .SelectMany(c => c.Integrantes)
        .Where(i => i.UsuarioToken == usuarioToken)
        .SelectMany(i => i.SomaPagamentosPorMetodo)
        .GroupBy(kvp => kvp.Key)
        .Select(g => new MetodosRelatorioResponseDTO
        {
            MetodoPagamento = g.Key, 
            TotalPago = g.Sum(kvp => kvp.Value)
        })
        .ToList();

    foreach (var compromisso in compromissosData)
    {
        foreach (var participacao in compromisso.Integrantes)
        {
            if (participacao.UsuarioToken == usuarioToken)
            {
                totalPagoResponse += participacao.ValorPago;
                totalDevidoResponse += participacao.ValorRestante;
                
                if (!participacao.Quitado)
                {
                    var compromissoEmAberto = new CompromissoEmAbertoRelatorioResponseDTO
                    {
                        Nome = compromisso.Titulo,
                        NomeGrupo = compromisso.NomeGrupo,
                        DataVencimento = compromisso.DataVencimento,
                        ValorEmAberto = participacao.ValorRestante
                    };
                    
                    compromissosEmAbertoResponse.Add(compromissoEmAberto);
                }
            }
        }
    }

    return (totalPagoResponse, totalDevidoResponse, nomeGrupoResponse, gruposResponse, compromissosEmAbertoResponse, totaisPagosPorCategoriaResponse, totaisPagosPorMetodoResponse);
}
    
    private byte[] GeneratePdf(RelatorioResponseDTO responseDto)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // 1. HEADER SECTION
                page.Header().Element(header => 
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Relatório Financeiro TribeWallet")
                                .Bold().FontSize(20).FontColor(Colors.Blue.Medium);

                            string subTitle = "";
                            if (responseDto is RelatorioResponseByUsuartioDTO userDto && !string.IsNullOrEmpty(userDto.NomeCompleto))
                            {
                                subTitle += $"Usuário: {userDto.NomeCompleto}";
                            }
                            else if (responseDto is RelatorioResponseByGrupoDTO groupDto && !string.IsNullOrEmpty(groupDto.NomeGrupo))
                            {
                                subTitle += $"Grupo: {groupDto.NomeGrupo} - Usuário: {groupDto.NomeUsuario}";
                            }

                            col.Item().Text(subTitle).FontSize(12).SemiBold().FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"Data de emissão: {responseDto.DataHora:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });
                });

                // 2. MAIN CONTENT SECTION
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // --- FINANCIAL SUMMARY CARDS ---
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Pago").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"R$ {responseDto.TotalPago:N2}").Bold().FontSize(16).FontColor(Colors.Green.Darken2);
                        });
                        row.ConstantItem(10); // spacing
                        row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Devido").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"R$ {responseDto.TotalDevido:N2}").Bold().FontSize(16).FontColor(Colors.Red.Darken2);
                        });
                    });

                    // --- USER GROUPS (If User Report DTO) ---
                    if (responseDto is RelatorioResponseByUsuartioDTO userReportDto && userReportDto.Grupos?.Any() == true)
                    {
                        column.Item().Text("Totais Pagos Para Cada Grupo").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nome do Grupo").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total").Bold();
                            });

                            foreach (var grupo in userReportDto.Grupos)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(grupo.NomeGrupo);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"R$ {grupo.TotalPago:N2}");
                            }
                        });
                    }

                    // --- CATEGORIES BREAKDOWN ---
                    if (responseDto.TotaisPagosPorCategorias?.Any() == true)
                    {
                        column.Item().Text("Totais Pagos Por Categoria").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoria").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total").Bold();
                            });

                            foreach (var cat in responseDto.TotaisPagosPorCategorias)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(cat.Categoria);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"R$ {cat.ValorPagoTotal:N2}");
                            }
                        });
                    }

                    // --- PAYMENT METHODS BREAKDOWN ---
                    if (responseDto.TotaisPagosPorMetodos?.Any() == true)
                    {
                        column.Item().Text("Totais Pagos Por Métodos de Pagamento").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Método").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total").Bold();
                            });

                            foreach (var method in responseDto.TotaisPagosPorMetodos)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(method.MetodoPagamento.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"R$ {method.TotalPago:N2}");
                            }
                        });
                    }

                    // --- OPEN COMMITMENTS ---
                    if (responseDto.CompromissosEmAberto?.Any() == true)
                    {
                        column.Item().Text("Compromissos em Aberto").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nome").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Grupo").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Data de Vencimento").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total em Aberto").Bold();
                            });

                            foreach (var item in responseDto.CompromissosEmAberto)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Nome);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.NomeGrupo);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.DataVencimento.ToString("dd/MM/yyyy"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"R$ {item.ValorEmAberto:N2}");
                            }
                        });
                    }
                });

                // 3. FOOTER SECTION
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}