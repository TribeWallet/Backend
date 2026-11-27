using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TribeWallet.Tests.Api;

public class CompromissoTests(ApiFixture api) : ApiTestBase(api)
{
    [Fact(DisplayName = "Compromisso criado guarda os dados informados")]
    public async Task Criar()
    {
        var c = await NovoCenario();
        var r = await Compromisso(c, await NovoCompromisso(c, "123.45"));
        Assert.Equal(123.45m, r.GetProperty("valorTotal").GetDecimal());
        Assert.Equal("Mercado", r.GetProperty("titulo").GetString());
    }

    [Theory(DisplayName = "Tipo de divisão fora de 1 a 4 é recusado")]
    [InlineData(0)]
    [InlineData(99)]
    public async Task DivisaoInvalida(int tipo)
    {
        var c = await NovoCenario();
        await Status(await c.Dono.Http.PostAsync($"/api/compromissos/{c.Grupo}", FormCompromisso(divisao: tipo)), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Título vazio é recusado")]
    public async Task TituloVazio()
    {
        var c = await NovoCenario();
        await Status(await c.Dono.Http.PostAsync($"/api/compromissos/{c.Grupo}", FormCompromisso(titulo: " ")), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Incluir participante registra o valor devido")]
    public async Task Incluir()
    {
        var c = await NovoCenario();
        var (compromisso, _) = await NovaFatia(c, 40m);
        var p = (await Compromisso(c, compromisso)).GetProperty("participacoes")[0];
        Assert.Equal(40m, p.GetProperty("valorDevedor").GetDecimal());
        Assert.Equal(0m, p.GetProperty("valorPago").GetDecimal());
    }

    [Fact(DisplayName = "Incluir o mesmo participante duas vezes não duplica")]
    public async Task IncluirDuplicado()
    {
        var c = await NovoCenario();
        var (compromisso, _) = await NovaFatia(c);
        await Sucesso(await c.Dono.Http.PostAsJsonAsync($"/api/compromissos/{compromisso}/integrantes", new[] { Parte(c.IntegranteMembro, 50) }));
        Assert.Equal(1, (await Compromisso(c, compromisso)).GetProperty("participacoes").GetArrayLength());
    }

    [Fact(DisplayName = "Remover participante e incluir de novo reativa a participação")]
    public async Task Reincluir()
    {
        var c = await NovoCenario();
        var (compromisso, fatia) = await NovaFatia(c);
        await Status(await c.Dono.Http.DeleteAsync($"/api/compromissos/integrantes/{fatia}"), HttpStatusCode.NoContent);
        Assert.Equal(0, (await Compromisso(c, compromisso)).GetProperty("participacoes").GetArrayLength());
        await Sucesso(await c.Dono.Http.PostAsJsonAsync($"/api/compromissos/{compromisso}/integrantes", new[] { Parte(c.IntegranteMembro, 50) }));
        Assert.Equal(1, (await Compromisso(c, compromisso)).GetProperty("participacoes").GetArrayLength());
    }

    [Fact(DisplayName = "Remover participação inexistente é recusado")]
    public async Task RemoverInexistente()
    {
        var conta = await NovaConta();
        await Recusado(await conta.Http.DeleteAsync($"/api/compromissos/integrantes/{Unico()}"));
    }

    private async Task<List<decimal>> Redividir(int tipo, string total, decimal parteDono, decimal parteMembro)
    {
        var c = await NovoCenario();
        var compromisso = await NovoCompromisso(c, total, divisao: tipo);
        await Sucesso(await c.Dono.Http.PostAsJsonAsync($"/api/compromissos/{compromisso}/integrantes",
            new[] { Parte(c.IntegranteDono, 1), Parte(c.IntegranteMembro, 1) }));
        await Sucesso(await c.Dono.Http.PutAsync($"/api/compromissos/{compromisso}", Form(
            ("ValorTotal", total), ("TipoDivisao", tipo.ToString()),
            ("Participacoes[0].IntegranteToken", c.IntegranteDono), ("Participacoes[0].ValorDevedor", parteDono.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("Participacoes[1].IntegranteToken", c.IntegranteMembro), ("Participacoes[1].ValorDevedor", parteMembro.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
        return (await Compromisso(c, compromisso)).GetProperty("participacoes").EnumerateArray()
            .Select(p => p.GetProperty("valorDevedor").GetDecimal()).OrderBy(v => v).ToList();
    }

    [Fact(DisplayName = "Divisão igual reparte o total entre os participantes")]
    public async Task DivisaoIgual() => Assert.Equal([100m, 100m], await Redividir(1, "200", 1, 1));

    [Fact(DisplayName = "Divisão por valor exato respeita os valores informados")]
    public async Task DivisaoExata() => Assert.Equal([30m, 70m], await Redividir(3, "100", 30, 70));

    [Fact(DisplayName = "Edição parcial mantém os demais campos")]
    public async Task EdicaoParcial()
    {
        var c = await NovoCenario();
        var token = await NovoCompromisso(c);
        await Sucesso(await c.Dono.Http.PutAsync($"/api/compromissos/{token}", Form(("Titulo", "Só o título"))));
        var r = await Compromisso(c, token);
        Assert.Equal("Só o título", r.GetProperty("titulo").GetString());
        Assert.Equal(100m, r.GetProperty("valorTotal").GetDecimal());
    }

    [Fact(DisplayName = "Excluir compromisso tira da lista ativa e mantém com deleted=true")]
    public async Task Excluir()
    {
        var c = await NovoCenario();
        var (token, _) = await NovaFatia(c);
        await Status(await c.Dono.Http.DeleteAsync($"/api/compromissos/{token}"), HttpStatusCode.NoContent);
        Assert.Empty((await Json(await c.Dono.Http.GetAsync($"/api/compromissos/{c.Grupo}?deleted=false"))).EnumerateArray());
        Assert.Single((await Json(await c.Dono.Http.GetAsync($"/api/compromissos/{c.Grupo}?deleted=true"))).EnumerateArray());
    }

    [Fact(DisplayName = "Excluir duas vezes é recusado na segunda")]
    public async Task ExcluirDuasVezes()
    {
        var c = await NovoCenario();
        var token = await NovoCompromisso(c);
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/compromissos/{token}"));
        await Recusado(await c.Dono.Http.DeleteAsync($"/api/compromissos/{token}"));
    }

    [Theory(DisplayName = "Situação do pagamento acompanha o valor pago")]
    [InlineData(null, 1)]
    [InlineData("40", 3)]
    [InlineData("100", 2)]
    public async Task Situacao(string? pago, int esperado)
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c, 100m);
        if (pago != null) await NovoPagamento(c.Membro, fatia, pago);
        var r = await Json(await c.Membro.Http.GetAsync($"/api/compromissos/usuario/{c.Membro.Token}"));
        Assert.Equal(esperado, r[0].GetProperty("situacaoPagamento").GetInt32());
    }

    [Fact(DisplayName = "Listagem por integrante traz os compromissos dele")]
    public async Task PorIntegrante()
    {
        var c = await NovoCenario();
        await NovaFatia(c);
        Assert.Single((await Json(await c.Membro.Http.GetAsync($"/api/compromissos/integrantes/{c.IntegranteMembro}"))).EnumerateArray());
    }
}

public class PagamentoTests(ApiFixture api) : ApiTestBase(api)
{
    private async Task<decimal> ValorPago(Cenario c, string compromisso) =>
        (await Compromisso(c, compromisso)).GetProperty("participacoes")[0].GetProperty("valorPago").GetDecimal();

    [Fact(DisplayName = "Registrar pagamento devolve 201 e soma no valor pago")]
    public async Task Registrar()
    {
        var c = await NovoCenario();
        var (compromisso, fatia) = await NovaFatia(c);
        await Status(await Pagar(c.Membro.Http, fatia, "20.00"), HttpStatusCode.Created);
        Assert.Equal(20m, await ValorPago(c, compromisso));
    }

    [Fact(DisplayName = "Pagamentos parciais são somados")]
    public async Task Parciais()
    {
        var c = await NovoCenario();
        var (compromisso, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "10.50");
        await NovoPagamento(c.Membro, fatia, "15.25");
        Assert.Equal(25.75m, await ValorPago(c, compromisso));
    }

    [Theory(DisplayName = "Aceita todos os métodos de pagamento")]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public async Task Metodos(int metodo)
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        var r = await Json(await Pagar(c.Membro.Http, fatia, "1.00", metodo));
        Assert.Equal(metodo, r.GetProperty("metodo").GetInt32());
    }

    [Fact(DisplayName = "Método de pagamento inexistente é recusado")]
    public async Task MetodoInvalido()
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        await Status(await Pagar(c.Membro.Http, fatia, "1.00", 99), HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "Valor zero ou negativo é recusado")]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task ValorInvalido(string valor)
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        await Status(await Pagar(c.Membro.Http, fatia, valor), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Pagamento para fatia inexistente é recusado")]
    public async Task FatiaInexistente()
    {
        var conta = await NovaConta();
        await Status(await Pagar(conta.Http, Unico(), "10"), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Editar com novo comprovante atualiza valor e total pago")]
    public async Task Editar()
    {
        var c = await NovoCenario();
        var (compromisso, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "10");
        var segundo = await NovoPagamento(c.Membro, fatia, "5");
        await Sucesso(await c.Membro.Http.PutAsync($"/api/Pagamento/{segundo}", ComArquivo(Form(("Valor", "20")), "Comprovante")));
        Assert.Equal(30m, await ValorPago(c, compromisso));
    }

    [Fact(DisplayName = "Excluir pagamento desconta do total pago")]
    public async Task Excluir()
    {
        var c = await NovoCenario();
        var (compromisso, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "10");
        var segundo = await NovoPagamento(c.Membro, fatia, "5");
        await Status(await c.Membro.Http.DeleteAsync($"/api/Pagamento/{segundo}"), HttpStatusCode.NoContent);
        Assert.Equal(10m, await ValorPago(c, compromisso));
    }

    [Fact(DisplayName = "Excluir pagamento inexistente devolve 404")]
    public async Task ExcluirInexistente()
    {
        var conta = await NovaConta();
        await Status(await conta.Http.DeleteAsync($"/api/Pagamento/{Unico()}"), HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Editar pagamento inexistente devolve 404")]
    public async Task EditarInexistente()
    {
        var conta = await NovaConta();
        await Status(await conta.Http.PutAsync($"/api/Pagamento/{Unico()}", ComArquivo(Form(("Valor", "1")), "Comprovante")), HttpStatusCode.NotFound);
    }
}

public class DashboardRelatorioTests(ApiFixture api) : ApiTestBase(api)
{
    private async Task<JsonElement> Painel(Conta quem) => await Json(await quem.Http.GetAsync($"/api/usuarios/{quem.Token}/dashboard"));

    [Fact(DisplayName = "Dashboard de usuário novo vem zerado")]
    public async Task Zerado()
    {
        var d = await Painel(await NovaConta());
        Assert.Equal(0, d.GetProperty("gruposAtivosCount").GetInt32());
        Assert.Equal(0, d.GetProperty("compromissosCountTotal").GetInt32());
        Assert.Equal(0m, d.GetProperty("valorTotalPago").GetDecimal());
    }

    [Theory(DisplayName = "Dashboard conta vencidos e a vencer")]
    [InlineData(-5, 1, 0)]
    [InlineData(3, 0, 1)]
    [InlineData(30, 0, 0)]
    public async Task Vencimentos(int dias, int vencidos, int aVencer)
    {
        var c = await NovoCenario();
        await NovaFatia(c, dias: dias);
        var d = await Painel(c.Membro);
        Assert.Equal(vencidos, d.GetProperty("compromissosVencidosCount").GetInt32());
        Assert.Equal(aVencer, d.GetProperty("compromissosAVencerCount").GetInt32());
        Assert.Equal(1, d.GetProperty("pagamentosPendentesCount").GetInt32());
    }

    [Fact(DisplayName = "Dashboard soma o valor pago")]
    public async Task Pago()
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "50");
        var d = await Painel(c.Membro);
        Assert.Equal(50m, d.GetProperty("valorTotalPago").GetDecimal());
        Assert.Equal(0, d.GetProperty("pagamentosPendentesCount").GetInt32());
    }

    [Fact(DisplayName = "Dashboard ignora compromisso excluído")]
    public async Task IgnoraExcluido()
    {
        var c = await NovoCenario();
        var (token, _) = await NovaFatia(c);
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/compromissos/{token}"));
        Assert.Equal(0, (await Painel(c.Membro)).GetProperty("compromissosCountTotal").GetInt32());
    }

    [Theory(DisplayName = "Relatórios em PDF devolvem um PDF de verdade")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Pdf(bool porGrupo)
    {
        var c = await NovoCenario();
        await NovaFatia(c);
        var rota = $"/api/relatorios/usuario/{c.Membro.Token}" + (porGrupo ? $"/grupo/{c.Grupo}" : "") + "/pdf";
        var r = await c.Membro.Http.GetAsync(rota);
        await Sucesso(r);
        Assert.Equal("application/pdf", r.Content.Headers.ContentType?.MediaType);
        Assert.Equal("%PDF-"u8.ToArray(), (await r.Content.ReadAsByteArrayAsync())[..5]);
    }

    [Fact(DisplayName = "Relatório de usuário sem compromissos não quebra")]
    public async Task RelatorioVazio()
    {
        var conta = await NovaConta();
        await Sucesso(await conta.Http.GetAsync($"/api/relatorios/usuario/{conta.Token}"));
        await Sucesso(await conta.Http.GetAsync($"/api/relatorios/usuario/{conta.Token}/pdf"));
    }
}

public class NotificacaoTests(ApiFixture api) : ApiTestBase(api)
{
    [Fact(DisplayName = "Participante incluído é avisado; quem incluiu não recebe o próprio aviso")]
    public async Task NovoCompromissoAviso()
    {
        var c = await NovoCenario();
        await NovaFatia(c);
        Assert.Contains(await Notificacoes(c.Membro), n => Tipo(n) == 2);
        Assert.DoesNotContain(await Notificacoes(c.Dono), n => Tipo(n) == 2);
    }

    [Fact(DisplayName = "Quitação gera aviso de pagamento e de compromisso quitado")]
    public async Task Quitado()
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "50");
        var avisos = await Notificacoes(c.Membro);
        Assert.Contains(avisos, n => Tipo(n) == 5);
    }

    [Fact(DisplayName = "Alterar compromisso avisa os participantes")]
    public async Task Alteracao()
    {
        var c = await NovoCenario();
        var (token, _) = await NovaFatia(c);
        await Sucesso(await c.Dono.Http.PutAsync($"/api/compromissos/{token}", Form(("ValorTotal", "300"))));
        Assert.Contains(await Notificacoes(c.Membro), n => Tipo(n) == 6);
    }

    [Fact(DisplayName = "Cobrança de vencimento aparece uma única vez no dia")]
    public async Task Cobranca()
    {
        var c = await NovoCenario();
        await NovaFatia(c, dias: 1);
        await Notificacoes(c.Membro);
        Assert.Single(await Notificacoes(c.Membro), n => Tipo(n) == 4);
    }

    [Fact(DisplayName = "Sem cobrança para vencimento distante")]
    public async Task SemCobranca()
    {
        var c = await NovoCenario();
        await NovaFatia(c, dias: 30);
        Assert.DoesNotContain(await Notificacoes(c.Membro), n => Tipo(n) == 4);
    }

    private async Task<(Cenario, string)> ComAviso()
    {
        var c = await NovoCenario();
        await NovaFatia(c);
        return (c, (await Notificacoes(c.Membro))[0].GetProperty("notificacaoToken").GetString()!);
    }

    private async Task<int> NaoLidas(Conta quem) =>
        (await Json(await quem.Http.GetAsync("/api/notificacoes/nao-lidas/contagem"))).GetProperty("naoLidas").GetInt32();

    [Fact(DisplayName = "Marcar como lida reduz a contagem")]
    public async Task MarcarLida()
    {
        var (c, token) = await ComAviso();
        var antes = await NaoLidas(c.Membro);
        await Status(await c.Membro.Http.PatchAsync($"/api/notificacoes/{token}/lida", null), HttpStatusCode.NoContent);
        Assert.Equal(antes - 1, await NaoLidas(c.Membro));
    }

    [Fact(DisplayName = "Marcar todas como lidas zera a contagem")]
    public async Task MarcarTodas()
    {
        var (c, _) = await ComAviso();
        await Status(await c.Membro.Http.PatchAsync("/api/notificacoes/lidas", null), HttpStatusCode.NoContent);
        Assert.Equal(0, await NaoLidas(c.Membro));
    }

    [Fact(DisplayName = "Excluir notificação tira da lista")]
    public async Task ExcluirAviso()
    {
        var (c, token) = await ComAviso();
        await Status(await c.Membro.Http.DeleteAsync($"/api/notificacoes/{token}"), HttpStatusCode.NoContent);
        Assert.DoesNotContain(await Notificacoes(c.Membro), n => n.GetProperty("notificacaoToken").GetString() == token);
    }

    [Theory(DisplayName = "Notificação de outro usuário responde 404")]
    [InlineData("PATCH", "/lida")]
    [InlineData("DELETE", "")]
    public async Task DeOutro(string metodo, string sufixo)
    {
        var (c, token) = await ComAviso();
        await Status(await c.Dono.Http.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), $"/api/notificacoes/{token}{sufixo}")), HttpStatusCode.NotFound);
    }
}
