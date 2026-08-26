using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TribeWallet.Entities;

namespace TribeWallet.Data;

/// <summary>
/// Popula o banco com um cenário de demonstração: dois grupos, cinco usuários, despesas
/// rateadas, pagamentos com comprovante, relatórios, notificações e auditoria.
/// É idempotente — se já existir qualquer usuário, não faz nada.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>Custo do BCrypt. Cada incremento dobra o tempo de verificação.</summary>
    private const int FatorBCrypt = 12;

    public static async Task SemearAsync(
        AppDbContext context,
        string senhaPadrao,
        string storageBaseUrl,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await context.Usuarios.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed ignorado: o banco já contém usuários.");
            return;
        }

        logger.LogInformation("Executando seed do TribeWallet...");

        // Data de referência para os registros ficarem com datas plausíveis entre si.
        var hoje = DateTime.UtcNow.Date;
        var senha = BCrypt.Net.BCrypt.HashPassword(senhaPadrao, FatorBCrypt);
        string Arquivo(string caminho) => $"{storageBaseUrl.TrimEnd('/')}/{caminho.TrimStart('/')}";

        // ------------------------------------------------------------------ usuários
        var ana = new Usuario { Nome = "Ana Beatriz", Sobrenome = "Ribeiro", Email = "ana.ribeiro@tribewallet.dev", Username = "ana.ribeiro", HashSenha = senha, Imagem = Arquivo("avatares/ana.png") };
        var bruno = new Usuario { Nome = "Bruno", Sobrenome = "Carvalho", Email = "bruno.carvalho@tribewallet.dev", Username = "bruno.carvalho", HashSenha = senha, Imagem = Arquivo("avatares/bruno.png") };
        var camila = new Usuario { Nome = "Camila", Sobrenome = "Souza", Email = "camila.souza@tribewallet.dev", Username = "camila.souza", HashSenha = senha, Imagem = Arquivo("avatares/camila.png") };
        var diego = new Usuario { Nome = "Diego", Sobrenome = "Fernandes", Email = "diego.fernandes@tribewallet.dev", Username = "diego.fernandes", HashSenha = senha };
        var elisa = new Usuario { Nome = "Elisa", Sobrenome = "Nogueira", Email = "elisa.nogueira@tribewallet.dev", Username = "elisa.nogueira", HashSenha = senha, Imagem = Arquivo("avatares/elisa.png") };

        context.Usuarios.AddRange(ana, bruno, camila, diego, elisa);

        // ------------------------------------------------------------------ grupos
        var republica = new Grupo { Nome = "República Vila Mariana", Descricao = "Aluguel, contas e mercado da casa compartilhada." };
        var viagem = new Grupo { Nome = "Viagem Chapada 2026", Descricao = "Rateio da viagem de fim de ano para a Chapada Diamantina." };

        context.Grupos.AddRange(republica, viagem);

        // ------------------------------------------------------------------ integrantes
        var anaRep = new Integrante { UsuarioId = ana.UsuarioId, GrupoId = republica.GrupoId };
        var brunoRep = new Integrante { UsuarioId = bruno.UsuarioId, GrupoId = republica.GrupoId };
        var camilaRep = new Integrante { UsuarioId = camila.UsuarioId, GrupoId = republica.GrupoId };
        var diegoRep = new Integrante { UsuarioId = diego.UsuarioId, GrupoId = republica.GrupoId };

        var anaVia = new Integrante { UsuarioId = ana.UsuarioId, GrupoId = viagem.GrupoId };
        var camilaVia = new Integrante { UsuarioId = camila.UsuarioId, GrupoId = viagem.GrupoId };
        var elisaVia = new Integrante { UsuarioId = elisa.UsuarioId, GrupoId = viagem.GrupoId };

        context.Integrantes.AddRange(anaRep, brunoRep, camilaRep, diegoRep, anaVia, camilaVia, elisaVia);

        // ------------------------------------------------------------------ compromissos
        var aluguel = Compromisso(republica, "Aluguel de agosto", 3_200.00m, hoje.AddDays(-24), TipoDivisao.Igual, "Moradia");
        var energia = Compromisso(republica, "Conta de luz", 289.40m, hoje.AddDays(-18), TipoDivisao.Igual, "Utilidades", Arquivo("comprovantes/conta-luz-agosto.jpg"));
        var mercado = Compromisso(republica, "Compras do mês", 742.85m, hoje.AddDays(-11), TipoDivisao.Proporcional, "Mercado", Arquivo("comprovantes/mercado-agosto.jpg"));
        var hospedagem = Compromisso(viagem, "Airbnb — 4 diárias", 1_860.00m, hoje.AddDays(-6), TipoDivisao.Igual, "Hospedagem");
        var combustivel = Compromisso(viagem, "Combustível e pedágio", 420.00m, hoje.AddDays(-2), TipoDivisao.ValorExato, "Transporte");

        context.CompromissosFinanceiros.AddRange(aluguel, energia, mercado, hospedagem, combustivel);

        // ------------------------------------------------------------------ rateios
        var rateioAluguel = RatearIgualmente(aluguel, [anaRep, brunoRep, camilaRep, diegoRep]);
        var rateioEnergia = RatearIgualmente(energia, [anaRep, brunoRep, camilaRep, diegoRep]);
        var rateioHospedagem = RatearIgualmente(hospedagem, [anaVia, camilaVia, elisaVia]);

        // Proporcional: Diego viajou metade do mês e paga meia cota.
        var rateioMercado = new[]
        {
            Participacao(anaRep, mercado, 212.24m),
            Participacao(brunoRep, mercado, 212.24m),
            Participacao(camilaRep, mercado, 212.25m),
            Participacao(diegoRep, mercado, 106.12m)
        };

        // Valor exato: cada uma acertou uma parte combinada na mão.
        var rateioCombustivel = new[]
        {
            Participacao(anaVia, combustivel, 140.00m),
            Participacao(camilaVia, combustivel, 160.00m),
            Participacao(elisaVia, combustivel, 120.00m)
        };

        context.IntegrantesCompromissos.AddRange(
            [.. rateioAluguel, .. rateioEnergia, .. rateioMercado, .. rateioHospedagem, .. rateioCombustivel]);

        // ------------------------------------------------------------------ pagamentos
        var pagamentos = new List<Pagamento>();

        // Aluguel: todo mundo quitou.
        for (var i = 0; i < rateioAluguel.Length; i++)
        {
            pagamentos.Add(Quitar(rateioAluguel[i], hoje.AddDays(-23 + i), MetodoPagamento.Pix, Arquivo($"comprovantes/aluguel-{i + 1}.pdf")));
        }

        // Luz: Ana e Camila pagaram; Bruno e Diego seguem em aberto.
        pagamentos.Add(Quitar(rateioEnergia[0], hoje.AddDays(-17), MetodoPagamento.Pix, Arquivo("comprovantes/luz-ana.pdf")));
        pagamentos.Add(Quitar(rateioEnergia[2], hoje.AddDays(-15), MetodoPagamento.TransferenciaBancaria, Arquivo("comprovantes/luz-camila.pdf")));

        // Mercado: Ana quitou; Bruno pagou em duas parcelas e ainda deve o resto.
        pagamentos.Add(Pagar(rateioMercado[0], 212.24m, hoje.AddDays(-10), MetodoPagamento.Dinheiro, null));
        pagamentos.Add(Pagar(rateioMercado[1], 100.00m, hoje.AddDays(-9), MetodoPagamento.Pix, Arquivo("comprovantes/mercado-bruno-1.pdf")));
        pagamentos.Add(Pagar(rateioMercado[1], 60.00m, hoje.AddDays(-4), MetodoPagamento.Pix, Arquivo("comprovantes/mercado-bruno-2.pdf")));

        // Hospedagem: só Elisa adiantou. Combustível: recém-lançado, ninguém acertou.
        pagamentos.Add(Quitar(rateioHospedagem[2], hoje.AddDays(-5), MetodoPagamento.CartaoCredito, Arquivo("comprovantes/airbnb-elisa.pdf")));

        context.Pagamentos.AddRange(pagamentos);
        var pagamentoElisa = pagamentos[^1];

        // ------------------------------------------------------------------ relatórios
        context.Relatorios.AddRange(
            new Relatorio { UsuarioId = ana.UsuarioId, CompromissoId = aluguel.CompromissoFinanceiroId, Tipo = TipoRelatorio.DetalhesCompromisso, DataHora = hoje.AddDays(-20).AddHours(9), ConteudoUrl = Arquivo("relatorios/aluguel-agosto.pdf") },
            new Relatorio { UsuarioId = camila.UsuarioId, CompromissoId = energia.CompromissoFinanceiroId, Tipo = TipoRelatorio.Pendencias, DataHora = hoje.AddDays(-14).AddHours(20), ConteudoUrl = Arquivo("relatorios/luz-pendencias.pdf") },
            new Relatorio { UsuarioId = elisa.UsuarioId, CompromissoId = hospedagem.CompromissoFinanceiroId, Tipo = TipoRelatorio.ExtratoIndividual, DataHora = hoje.AddDays(-5).AddHours(18), ConteudoUrl = Arquivo("relatorios/airbnb-elisa.pdf") },
            // Ainda sendo gerado: o arquivo não subiu para o storage.
            new Relatorio { UsuarioId = ana.UsuarioId, CompromissoId = mercado.CompromissoFinanceiroId, Tipo = TipoRelatorio.ResumoGrupo, DataHora = hoje.AddHours(-3) });

        // ------------------------------------------------------------------ notificações
        context.Notificacoes.AddRange(
            new Notificacao { UsuarioDestinoId = bruno.UsuarioId, UsuarioOrigemId = ana.UsuarioId, Entidade = nameof(Grupo), EntidadeId = republica.GrupoId, Tipo = TipoNotificacao.ConviteGrupo, Mensagem = "Ana convidou você para o grupo República Vila Mariana.", DataEnvio = hoje.AddDays(-30).AddHours(10), Lida = true },
            new Notificacao { UsuarioDestinoId = bruno.UsuarioId, UsuarioOrigemId = ana.UsuarioId, Entidade = nameof(CompromissoFinanceiro), EntidadeId = energia.CompromissoFinanceiroId, Tipo = TipoNotificacao.CobrancaPendente, Mensagem = "Sua parte da conta de luz (R$ 72,35) está em aberto.", DataEnvio = hoje.AddDays(-12).AddHours(8) },
            new Notificacao { UsuarioDestinoId = diego.UsuarioId, UsuarioOrigemId = ana.UsuarioId, Entidade = nameof(CompromissoFinanceiro), EntidadeId = energia.CompromissoFinanceiroId, Tipo = TipoNotificacao.CobrancaPendente, Mensagem = "Sua parte da conta de luz (R$ 72,35) está em aberto.", DataEnvio = hoje.AddDays(-12).AddHours(8) },
            new Notificacao { UsuarioDestinoId = ana.UsuarioId, UsuarioOrigemId = elisa.UsuarioId, Entidade = nameof(Pagamento), EntidadeId = pagamentoElisa.PagamentoId, Tipo = TipoNotificacao.PagamentoRegistrado, Mensagem = "Elisa registrou o pagamento de R$ 620,00 do Airbnb.", DataEnvio = hoje.AddDays(-5).AddHours(19) },
            // Sem remetente: gerada pelo próprio sistema.
            new Notificacao { UsuarioDestinoId = camila.UsuarioId, UsuarioOrigemId = null, Entidade = nameof(CompromissoFinanceiro), EntidadeId = aluguel.CompromissoFinanceiroId, Tipo = TipoNotificacao.CompromissoQuitado, Mensagem = "O aluguel de agosto foi totalmente quitado.", DataEnvio = hoje.AddDays(-20).AddHours(21) });

        // ------------------------------------------------------------------ auditoria
        context.HistoricoAlteracoes.AddRange(
            new HistoricoAlteracao { UsuarioId = ana.UsuarioId, Entidade = nameof(CompromissoFinanceiro), EntidadeId = aluguel.CompromissoFinanceiroId, Tipo = TipoAlteracao.Criacao, DadosDepois = Json(new { aluguel.Titulo, aluguel.ValorTotal, aluguel.TipoDivisao }), DataHora = hoje.AddDays(-24).AddHours(11) },
            new HistoricoAlteracao { UsuarioId = ana.UsuarioId, Entidade = nameof(CompromissoFinanceiro), EntidadeId = mercado.CompromissoFinanceiroId, Tipo = TipoAlteracao.Atualizacao, DadosAntes = Json(new { ValorTotal = 698.10m }), DadosDepois = Json(new { mercado.ValorTotal }), DataHora = hoje.AddDays(-10).AddHours(15) },
            new HistoricoAlteracao { UsuarioId = elisa.UsuarioId, Entidade = nameof(Pagamento), EntidadeId = pagamentoElisa.PagamentoId, Tipo = TipoAlteracao.Criacao, DadosDepois = Json(new { pagamentoElisa.Valor, pagamentoElisa.Metodo }), DataHora = hoje.AddDays(-5).AddHours(18) });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seed concluído: 5 usuários, 2 grupos, 5 compromissos, {Pagamentos} pagamentos.", pagamentos.Count);
    }

    private static CompromissoFinanceiro Compromisso(
        Grupo grupo, string titulo, decimal valor, DateTime data, TipoDivisao divisao, string categoria, string? imagem = null)
        => new()
        {
            GrupoId = grupo.GrupoId,
            Titulo = titulo,
            ValorTotal = valor,
            Data = data,
            TipoDivisao = divisao,
            Categoria = categoria,
            Imagem = imagem
        };

    private static IntegranteCompromisso Participacao(Integrante integrante, CompromissoFinanceiro compromisso, decimal devedor)
        => new()
        {
            IntegranteId = integrante.IntegranteId,
            CompromissoId = compromisso.CompromissoFinanceiroId,
            ValorDevedor = devedor
        };

    /// <summary>
    /// Divide o valor total igualmente entre os integrantes. A sobra dos centavos vai para a
    /// primeira fatia, então a soma dos rateios bate exatamente com o total do compromisso.
    /// </summary>
    private static IntegranteCompromisso[] RatearIgualmente(CompromissoFinanceiro compromisso, Integrante[] integrantes)
    {
        var centavos = (long)Math.Round(compromisso.ValorTotal * 100m, MidpointRounding.AwayFromZero);
        var porParte = centavos / integrantes.Length;
        var sobra = centavos - (porParte * integrantes.Length);

        return [.. integrantes.Select((integrante, i) =>
            Participacao(integrante, compromisso, (porParte + (i == 0 ? sobra : 0)) / 100m))];
    }

    /// <summary>Registra um pagamento parcial, já refletindo o valor no acumulado da participação.</summary>
    private static Pagamento Pagar(
        IntegranteCompromisso participacao, decimal valor, DateTime data, MetodoPagamento metodo, string? comprovante)
    {
        participacao.ValorPago += valor;

        return new Pagamento
        {
            IntegranteCompromissoId = participacao.IntegranteCompromissoId,
            Valor = valor,
            Data = DateTime.SpecifyKind(data, DateTimeKind.Utc),
            Metodo = metodo,
            ComprovanteUrl = comprovante
        };
    }

    /// <summary>Registra um pagamento que zera o saldo restante da participação.</summary>
    private static Pagamento Quitar(
        IntegranteCompromisso participacao, DateTime data, MetodoPagamento metodo, string? comprovante)
        => Pagar(participacao, participacao.ValorDevedor - participacao.ValorPago, data, metodo, comprovante);

    private static string Json(object valor) => JsonSerializer.Serialize(valor);
}
