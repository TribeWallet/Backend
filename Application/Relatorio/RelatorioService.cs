using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Relatorio.DTOs;
using TribeWallet.Application.Usuario;

namespace TribeWallet.Application.Relatorio;
using TribeWallet.Domain.Entities;

public class RelatorioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly ICompromissoFinanceiroRepository _compromissoFinanceiroRepository;
    private readonly UsuarioService _usuarioService;
    private readonly GrupoService _grupoService;
    private readonly CompromissoFinanceiroService _compromissoFinanceiroService;
    
    public RelatorioService(IUsuarioRepository usuarioRepository, IGrupoRepository grupoRepository, ICompromissoFinanceiroRepository compromissoFinanceiroRepository, UsuarioService usuarioService, GrupoService grupoService, CompromissoFinanceiroService compromissoFinanceiroService)
    {
        _usuarioRepository = usuarioRepository;
        _grupoRepository = grupoRepository;
        _compromissoFinanceiroRepository = compromissoFinanceiroRepository;
        _usuarioService = usuarioService;
        _grupoService = grupoService;
        _compromissoFinanceiroService = compromissoFinanceiroService;
    }

    public async Task<byte[]> GetByUserToken(string usuarioToken)
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
            Nome = "Despesas da República",
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

        var dataForPdf = PrepareDataByUsuario(dadosCompromissos, dadosUsuario);
        var reponseData = GeneratePdf(dataForPdf);
        return reponseData;
    }

    private RelatorioResponseByUsuartioDTO PrepareDataByUsuario(
        List<CompromissoFinanceiroResponseMainDTO> compromissosData, Usuario usuarioData)
    {
        var totais = GetGruposAndCompromissos(compromissosData, usuarioData.Token);
        var responseData = new RelatorioResponseByUsuartioDTO
        {
            NomeCompleto = $"{usuarioData.Nome} {usuarioData.Sobrenome}",
            Tipo = TipoRelatorio.ExtratoIndividual,
            DataHora = DateTime.Now,
            TotalPago = totais.totalPago,
            TotalDevido = totais.totalDevido,
            Grupos =  totais.grupos,
            TotaisPagosPorCategorias = totais.totaisPagosPorCategoria,
            TotaisPagosPorMetodos = totais.totaisPagosPorMetodo,
            CompromissosEmAberto = totais.compromissosEmAberto
        };

        return responseData;
    }
    
    private (decimal totalPago, decimal totalDevido, 
        List<UserGruposRelatorioResponseDTO> grupos,
        List<CompromissoEmAbertoRelatorioResponseDTO> compromissosEmAberto,
        List<CategoriasCompromissosRelatorioResponseDTO> totaisPagosPorCategoria, 
        List<MetodosRelatorioResponseDTO> totaisPagosPorMetodo
        ) GetGruposAndCompromissos(List<CompromissoFinanceiroResponseMainDTO> compromissosData, string usuarioToken)
    {
        decimal totalPagoResponse = 0;
        decimal totalDevidoResponse = 0;
        var compromissosEmAbertoResponse = new List<CompromissoEmAbertoRelatorioResponseDTO>();
        var totaisPagosPorCategoriaResponse = new List<CategoriasCompromissosRelatorioResponseDTO>();
        var totaisPagosPorMetodoResponse = new List<MetodosRelatorioResponseDTO>();

        var gruposResponse = compromissosData
            .SelectMany(c => c.Integrantes, (c, i) => new { c.GrupoToken, c.NomeGrupo, Integrante = i })
            .Where(x => x.Integrante.UsuarioToken == usuarioToken)
            .GroupBy(x => new { x.GrupoToken, x.NomeGrupo })
            .Select(g => new UserGruposRelatorioResponseDTO
            {
                NomeGrupo = g.Key.NomeGrupo,
                TotalPago = g.Sum(x => x.Integrante.ValorPago)
            })
            .ToList();
        
        totaisPagosPorCategoriaResponse = compromissosData
            .SelectMany(c => c.Integrantes, (c, i) => new { c.Categoria, Integrante = i })
            .Where(x => x.Integrante.UsuarioToken == usuarioToken)
            .GroupBy(x => x.Categoria)
            .Select(g => new CategoriasCompromissosRelatorioResponseDTO
            {
                Categoria = g.Key,
                ValorPagoTotal = g.Sum(x => x.Integrante.ValorPago)
            })
            .ToList();

        totaisPagosPorMetodoResponse = compromissosData
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
            foreach (var integrante in compromisso.Integrantes)
            {
                if (integrante.UsuarioToken == usuarioToken)
                {
                    totalPagoResponse += integrante.ValorPago;
                    totalDevidoResponse += integrante.ValorRestante;
                    
                    if (!integrante.Quitado)
                    {
                        var compromissoEmAberto = new CompromissoEmAbertoRelatorioResponseDTO
                        {
                            Nome = integrante.Nome,
                            DataVencimento = compromisso.DataVencimento,
                            ValorEmAberto = integrante.ValorDevedor
                        };
                        
                        compromissosEmAbertoResponse.Add(compromissoEmAberto);
                    }

                }
            }
        }

        return (totalPagoResponse, totalDevidoResponse, gruposResponse, compromissosEmAbertoResponse, totaisPagosPorCategoriaResponse, totaisPagosPorMetodoResponse);
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
                            else if (responseDto is RelatorioResponseByGrupoDTO groupDto && !string.IsNullOrEmpty(groupDto.Nome))
                            {
                                subTitle += $"Grupo: {groupDto.Nome}";
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
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Nome").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Data de Vencimento").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total em Aberto").Bold();
                            });

                            foreach (var item in responseDto.CompromissosEmAberto)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Nome);
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