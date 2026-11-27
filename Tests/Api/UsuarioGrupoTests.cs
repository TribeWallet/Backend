using System.Net;
using System.Net.Http.Json;

namespace TribeWallet.Tests.Api;

public class UsuarioTests(ApiFixture api) : ApiTestBase(api)
{
    [Fact(DisplayName = "Listagem de usuários não expõe hash de senha")]
    public async Task ListagemSemHash()
    {
        var conta = await NovaConta();
        var corpo = await (await conta.Http.GetAsync("/api/usuarios")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("hashSenha", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$2a$", corpo);
    }

    [Fact(DisplayName = "Busca por username encontra o usuário sem diferenciar maiúsculas")]
    public async Task Busca()
    {
        var conta = await NovaConta();
        var r = await Json(await conta.Http.GetAsync($"/api/usuarios/{conta.Username.ToUpper()}"));
        Assert.Equal(conta.Token, Assert.Single(r.EnumerateArray()).GetProperty("usuarioToken").GetString());
    }

    [Fact(DisplayName = "Busca sem resultado devolve lista vazia")]
    public async Task BuscaVazia()
    {
        var conta = await NovaConta();
        Assert.Empty((await Json(await conta.Http.GetAsync($"/api/usuarios/{Unico()}"))).EnumerateArray());
    }

    [Fact(DisplayName = "Usuário excluído some da busca e da listagem")]
    public async Task ExcluidoSome()
    {
        var alvo = await NovaConta();
        var outro = await NovaConta();
        await Sucesso(await alvo.Http.DeleteAsync($"/api/usuarios/{alvo.Token}"));
        Assert.Empty((await Json(await outro.Http.GetAsync($"/api/usuarios/{alvo.Username}"))).EnumerateArray());
        var todos = await (await outro.Http.GetAsync("/api/usuarios?deleted=false")).Content.ReadAsStringAsync();
        Assert.DoesNotContain(alvo.Token, todos);
    }

    [Fact(DisplayName = "Listagem com deleted=true inclui o excluído com a data de exclusão")]
    public async Task ListagemComExcluidos()
    {
        var alvo = await NovaConta();
        await Sucesso(await alvo.Http.DeleteAsync($"/api/usuarios/{alvo.Token}"));
        var todos = await Json(await Anonimo.GetAsync("/api/usuarios?deleted=true"));
        var achado = todos.EnumerateArray().Single(u => u.GetProperty("usuarioToken").GetString() == alvo.Token);
        Assert.NotNull(achado.GetProperty("deletedAt").GetString());
    }

    [Fact(DisplayName = "Edição parcial mantém os campos não enviados")]
    public async Task EdicaoParcial()
    {
        var conta = await NovaConta();
        var r = await Json(await conta.Http.PutAsync($"/api/usuarios/{conta.Token}", Form(("Nome", "Novo"))));
        Assert.Equal("Novo", r.GetProperty("nome").GetString());
        Assert.Equal("Teste", r.GetProperty("sobrenome").GetString());
        Assert.Equal(conta.Username, r.GetProperty("username").GetString());
    }

    [Fact(DisplayName = "Troca de senha pelo perfil passa a valer no login")]
    public async Task TrocaSenha()
    {
        var conta = await NovaConta();
        await Sucesso(await conta.Http.PutAsync($"/api/usuarios/{conta.Token}", Form(("Senha", "Nova@12345"))));
        await Sucesso(await Anonimo.PostAsJsonAsync("/api/auth/login", new { email = conta.Email, senha = "Nova@12345" }));
    }

    [Fact(DisplayName = "Troca de senha fraca pelo perfil é recusada")]
    public async Task TrocaSenhaFraca()
    {
        var conta = await NovaConta();
        await Status(await conta.Http.PutAsync($"/api/usuarios/{conta.Token}", Form(("Senha", "123"))), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Upload de foto PNG é aceito")]
    public async Task UploadFoto()
    {
        var conta = await NovaConta();
        var r = await Json(await conta.Http.PutAsync($"/api/usuarios/{conta.Token}", ComArquivo(new(), "Imagem", "foto.png")));
        Assert.EndsWith(".png", r.GetProperty("imageUrl").GetString());
    }

    [Theory(DisplayName = "Upload de tipo de arquivo perigoso é recusado")]
    [InlineData("script.exe", "application/octet-stream")]
    [InlineData("pagina.html", "text/html")]
    public async Task UploadPerigoso(string nome, string tipo)
    {
        var conta = await NovaConta();
        await Recusado(await conta.Http.PutAsync($"/api/usuarios/{conta.Token}",
            ComArquivo(new(), "Imagem", nome, tipo, "<script>alert(1)</script>")));
    }

    [Fact(DisplayName = "Excluir usuário inexistente é recusado")]
    public async Task ExcluirInexistente()
    {
        var conta = await NovaConta();
        await Recusado(await conta.Http.DeleteAsync($"/api/usuarios/{Unico()}"));
    }
}

public class GrupoTests(ApiFixture api) : ApiTestBase(api)
{
    private static Task<HttpResponseMessage> Adicionar(Conta quem, string grupo, string usuario) =>
        quem.Http.PutAsJsonAsync($"/api/grupos/{grupo}/integrantes", new[] { new { usuarioToken = usuario } });

    [Fact(DisplayName = "Grupo criado traz os integrantes informados")]
    public async Task Criar()
    {
        var c = await NovoCenario();
        var r = await Json(await c.Dono.Http.GetAsync($"/api/grupos/{c.Grupo}"));
        Assert.Equal(2, r.GetProperty("integrantes").GetArrayLength());
    }

    [Fact(DisplayName = "Grupo sem compromisso aparece na listagem")]
    public async Task ListaSemCompromisso()
    {
        var c = await NovoCenario();
        var r = await Json(await c.Dono.Http.GetAsync($"/api/grupos/usuario/{c.Dono.Token}"));
        var g = Assert.Single(r.EnumerateArray());
        Assert.Equal(2, g.GetProperty("integrantesCount").GetInt32());
        Assert.Equal(0, g.GetProperty("compromissosCount").GetInt32());
    }

    [Fact(DisplayName = "Totais do grupo refletem o que foi pago e o que está em aberto")]
    public async Task Totais()
    {
        var c = await NovoCenario();
        var (_, fatia) = await NovaFatia(c);
        await NovoPagamento(c.Membro, fatia, "20.00");
        var g = (await Json(await c.Dono.Http.GetAsync($"/api/grupos/usuario/{c.Dono.Token}")))[0];
        Assert.Equal(20m, g.GetProperty("totalPago").GetDecimal());
        Assert.Equal(30m, g.GetProperty("totalEmAberto").GetDecimal());
    }

    [Fact(DisplayName = "Edição altera nome e descrição")]
    public async Task Editar()
    {
        var c = await NovoCenario();
        var r = await Json(await c.Dono.Http.PutAsJsonAsync($"/api/grupos/{c.Grupo}", new { nome = "Renomeado", descricao = "nova" }));
        Assert.Equal("Renomeado", r.GetProperty("nome").GetString());
        Assert.Equal("nova", r.GetProperty("descricao").GetString());
    }

    [Fact(DisplayName = "Grupo excluído sai da listagem ativa e aparece com deleted=true")]
    public async Task Excluir()
    {
        var c = await NovoCenario();
        await Status(await c.Dono.Http.DeleteAsync($"/api/grupos/{c.Grupo}"), HttpStatusCode.NoContent);
        Assert.Empty((await Json(await c.Dono.Http.GetAsync($"/api/grupos/usuario/{c.Dono.Token}?deleted=false"))).EnumerateArray());
        var todos = await Json(await c.Dono.Http.GetAsync($"/api/grupos/usuario/{c.Dono.Token}?deleted=true"));
        Assert.NotNull(Assert.Single(todos.EnumerateArray()).GetProperty("deletedAt").GetString());
    }

    [Fact(DisplayName = "Adicionar novo integrante funciona")]
    public async Task AdicionarNovo()
    {
        var c = await NovoCenario();
        var novo = await NovaConta();
        await Sucesso(await Adicionar(c.Dono, c.Grupo, novo.Token));
    }

    [Fact(DisplayName = "Adicionar quem já é integrante é recusado")]
    public async Task AdicionarDuplicado()
    {
        var c = await NovoCenario();
        await Recusado(await Adicionar(c.Dono, c.Grupo, c.Membro.Token));
    }

    [Fact(DisplayName = "Remover integrante sem compromisso funciona")]
    public async Task Remover()
    {
        var c = await NovoCenario();
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/grupos/{c.Grupo}/integrantes/{c.IntegranteMembro}"));
    }
}

public class ConviteTests(ApiFixture api) : ApiTestBase(api)
{
    private async Task<string> Convidar(Cenario c, string? email = null)
    {
        var r = await Json(await c.Dono.Http.PostAsJsonAsync($"/api/grupos/{c.Grupo}/convites", new { email }));
        return r.GetProperty("conviteToken").GetString()!;
    }

    private static Task<HttpResponseMessage> Aceitar(Conta quem, string convite) =>
        quem.Http.PostAsync($"/api/convites/{convite}/aceitar", null);

    [Fact(DisplayName = "Integrante gera link que expira em 7 dias")]
    public async Task Gerar()
    {
        var c = await NovoCenario();
        var r = await Json(await c.Dono.Http.PostAsJsonAsync($"/api/grupos/{c.Grupo}/convites", new { }));
        Assert.EndsWith(r.GetProperty("conviteToken").GetString()!, r.GetProperty("link").GetString());
        var dias = (r.GetProperty("expiraEm").GetDateTime().ToUniversalTime() - DateTime.UtcNow).TotalDays;
        Assert.InRange(dias, 6.99, 7.01);
        Assert.False(r.GetProperty("emailEnviado").GetBoolean());
    }

    [Fact(DisplayName = "Convite com e-mail envia o link para o destinatário")]
    public async Task EnviaEmail()
    {
        var c = await NovoCenario();
        var email = $"{Unico()}@teste.dev";
        var token = await Convidar(c, email);
        Assert.Contains(token, Api.Email.Enviados.Last(e => e.Para == email).Corpo);
    }

    [Fact(DisplayName = "E-mail de convite inválido é recusado")]
    public async Task EmailInvalido()
    {
        var c = await NovoCenario();
        await Status(await c.Dono.Http.PostAsJsonAsync($"/api/grupos/{c.Grupo}/convites", new { email = "xxx" }), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Quem não é integrante não gera convite (403)")]
    public async Task IntrusoNaoConvida()
    {
        var c = await NovoCenario();
        var intruso = await NovaConta();
        await Status(await intruso.Http.PostAsJsonAsync($"/api/grupos/{c.Grupo}/convites", new { }), HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Dados do convite são públicos antes do login")]
    public async Task ConsultaPublica()
    {
        var c = await NovoCenario();
        var r = await Json(await Anonimo.GetAsync($"/api/convites/{await Convidar(c)}"));
        Assert.StartsWith("Grupo ", r.GetProperty("grupoNome").GetString());
    }

    [Fact(DisplayName = "Aceitar coloca o usuário no grupo")]
    public async Task AceitarConvite()
    {
        var c = await NovoCenario();
        var novo = await NovaConta();
        var r = await Json(await Aceitar(novo, await Convidar(c)));
        Assert.Equal(c.Grupo, r.GetProperty("grupoToken").GetString());
        Assert.Single((await Json(await novo.Http.GetAsync($"/api/grupos/usuario/{novo.Token}"))).EnumerateArray());
    }

    [Fact(DisplayName = "Aceitar duas vezes não duplica o integrante")]
    public async Task Idempotente()
    {
        var c = await NovoCenario();
        var novo = await NovaConta();
        var convite = await Convidar(c);
        await Sucesso(await Aceitar(novo, convite));
        await Sucesso(await Aceitar(novo, convite));
        var g = await Json(await c.Dono.Http.GetAsync($"/api/grupos/{c.Grupo}"));
        Assert.Equal(3, g.GetProperty("integrantes").GetArrayLength());
    }

    [Fact(DisplayName = "Convite restrito a um e-mail não serve para outra conta")]
    public async Task OutroEmail()
    {
        var c = await NovoCenario();
        var novo = await NovaConta();
        await Status(await Aceitar(novo, await Convidar(c, $"{Unico()}@teste.dev")), HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Convite restrito aceita o e-mail sem diferenciar maiúsculas")]
    public async Task EmailCaixaAlta()
    {
        var c = await NovoCenario();
        var novo = await NovaConta();
        await Sucesso(await Aceitar(novo, await Convidar(c, novo.Email.ToUpper())));
    }

    [Fact(DisplayName = "Quem saiu do grupo volta pelo convite")]
    public async Task Reativa()
    {
        var c = await NovoCenario();
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/grupos/{c.Grupo}/integrantes/{c.IntegranteMembro}"));
        await Sucesso(await Aceitar(c.Membro, await Convidar(c)));
    }

    [Fact(DisplayName = "Integrante removido não gera mais convites")]
    public async Task RemovidoNaoConvida()
    {
        var c = await NovoCenario();
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/grupos/{c.Grupo}/integrantes/{c.IntegranteMembro}"));
        await Status(await c.Membro.Http.PostAsJsonAsync($"/api/grupos/{c.Grupo}/convites", new { }), HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Convite expirado é recusado")]
    public async Task Expirado()
    {
        var c = await NovoCenario();
        var convite = await Convidar(c);
        await Api.Sql($"update convites_grupo set expira_em = now() - interval '1 minute' where token = {convite}");
        await Status(await Anonimo.GetAsync($"/api/convites/{convite}"), HttpStatusCode.BadRequest);
        await Status(await Aceitar(await NovaConta(), convite), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Convite de grupo excluído é recusado")]
    public async Task GrupoExcluido()
    {
        var c = await NovoCenario();
        var convite = await Convidar(c);
        await Sucesso(await c.Dono.Http.DeleteAsync($"/api/grupos/{c.Grupo}"));
        await Recusado(await Aceitar(await NovaConta(), convite));
    }

    [Fact(DisplayName = "Token de convite inexistente é recusado")]
    public async Task Inexistente() => await Recusado(await Anonimo.GetAsync($"/api/convites/{Unico()}"));
}
