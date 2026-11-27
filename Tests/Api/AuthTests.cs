using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TribeWallet.Tests.Api;

public class AuthTests(ApiFixture api) : ApiTestBase(api)
{
    [Fact(DisplayName = "Cadastro válido devolve os dados do usuário")]
    public async Task CadastroValido()
    {
        var id = Unico();
        var r = await Json(await Anonimo.PostAsJsonAsync("/api/auth/register", Cadastro(id)));
        Assert.Equal($"{id}@teste.dev", r.GetProperty("email").GetString());
        Assert.False(string.IsNullOrEmpty(r.GetProperty("usuarioToken").GetString()));
    }

    [Fact(DisplayName = "Cadastro não devolve senha nem hash")]
    public async Task CadastroNaoVazaSenha()
    {
        var r = await Anonimo.PostAsJsonAsync("/api/auth/register", Cadastro(Unico()));
        var corpo = await r.Content.ReadAsStringAsync();
        Assert.DoesNotContain("senha", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$2", corpo);
    }

    [Theory(DisplayName = "Cadastro recusa senha fora da política")]
    [InlineData("")]
    [InlineData("Ab@1")]
    [InlineData("Abc@12 ")]
    [InlineData("senha@123")]
    [InlineData("SENHA@123")]
    [InlineData("Senha@abc")]
    [InlineData("Senha1234")]
    [InlineData("Aa@1aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task SenhaInvalida(string senha) =>
        await Status(await Anonimo.PostAsJsonAsync("/api/auth/register", Cadastro(Unico(), senha)), HttpStatusCode.BadRequest);

    [Theory(DisplayName = "Cadastro aceita senhas nos limites da política")]
    [InlineData("Aa@12345")]
    [InlineData("Aa@1aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task SenhaNoLimite(string senha) =>
        await Sucesso(await Anonimo.PostAsJsonAsync("/api/auth/register", Cadastro(Unico(), senha)));

    [Fact(DisplayName = "Cadastro recusa e-mail já usado")]
    public async Task EmailDuplicado()
    {
        var conta = await NovaConta();
        var r = await Anonimo.PostAsJsonAsync("/api/auth/register",
            new { nome = "A", sobrenome = "B", email = conta.Email, username = "u" + Unico(), senha = Senha });
        await Status(r, HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Cadastro recusa username já usado")]
    public async Task UsernameDuplicado()
    {
        var conta = await NovaConta();
        var r = await Anonimo.PostAsJsonAsync("/api/auth/register",
            new { nome = "A", sobrenome = "B", email = $"{Unico()}@teste.dev", username = conta.Username, senha = Senha });
        await Status(r, HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Login devolve um JWT")]
    public async Task LoginValido()
    {
        var conta = await NovaConta();
        Assert.Equal(3, conta.Jwt.Split('.').Length);
    }

    [Fact(DisplayName = "Login com senha errada devolve 401")]
    public async Task LoginSenhaErrada()
    {
        var conta = await NovaConta();
        await Status(await Anonimo.PostAsJsonAsync("/api/auth/login", new { email = conta.Email, senha = "Errada@123" }), HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Login com e-mail inexistente devolve 401")]
    public async Task LoginInexistente() =>
        await Status(await Anonimo.PostAsJsonAsync("/api/auth/login", new { email = $"{Unico()}@teste.dev", senha = Senha }), HttpStatusCode.Unauthorized);

    [Theory(DisplayName = "Rotas protegidas exigem autenticação")]
    [InlineData("GET", "/api/usuarios/Nome")]
    [InlineData("PUT", "/api/usuarios/x")]
    [InlineData("DELETE", "/api/usuarios/x")]
    [InlineData("GET", "/api/usuarios/x/dashboard")]
    [InlineData("GET", "/api/grupos/x")]
    [InlineData("GET", "/api/grupos/usuario/x")]
    [InlineData("POST", "/api/grupos")]
    [InlineData("PUT", "/api/grupos/x")]
    [InlineData("DELETE", "/api/grupos/x")]
    [InlineData("PUT", "/api/grupos/x/integrantes")]
    [InlineData("DELETE", "/api/grupos/x/integrantes/y")]
    [InlineData("POST", "/api/grupos/x/convites")]
    [InlineData("POST", "/api/convites/x/aceitar")]
    [InlineData("GET", "/api/compromissos/x")]
    [InlineData("GET", "/api/compromissos/usuario/x")]
    [InlineData("GET", "/api/compromissos/integrantes/x")]
    [InlineData("POST", "/api/compromissos/x")]
    [InlineData("PUT", "/api/compromissos/x")]
    [InlineData("DELETE", "/api/compromissos/x")]
    [InlineData("POST", "/api/compromissos/x/integrantes")]
    [InlineData("DELETE", "/api/compromissos/integrantes/x")]
    [InlineData("POST", "/api/Pagamento")]
    [InlineData("PUT", "/api/Pagamento/x")]
    [InlineData("DELETE", "/api/Pagamento/x")]
    [InlineData("GET", "/api/relatorios/usuario/x")]
    [InlineData("GET", "/api/relatorios/usuario/x/pdf")]
    [InlineData("GET", "/api/relatorios/usuario/x/grupo/y")]
    [InlineData("GET", "/api/relatorios/usuario/x/grupo/y/pdf")]
    [InlineData("GET", "/api/notificacoes")]
    [InlineData("GET", "/api/notificacoes/nao-lidas/contagem")]
    [InlineData("PATCH", "/api/notificacoes/lidas")]
    [InlineData("PATCH", "/api/notificacoes/x/lida")]
    [InlineData("DELETE", "/api/notificacoes/x")]
    public async Task SemToken(string metodo, string rota) =>
        await Status(await Anonimo.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), rota)), HttpStatusCode.Unauthorized);

    [Theory(DisplayName = "JWT inválido ou adulterado é recusado")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TokenInvalido(bool adulterado)
    {
        var jwt = "isso.nao.e-um-token";
        if (adulterado)
        {
            var partes = (await NovaConta()).Jwt.Split('.');
            jwt = $"{partes[0]}.{partes[1]}.{new string('A', partes[2].Length)}";
        }
        var http = Api.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        await Status(await http.GetAsync("/api/notificacoes"), HttpStatusCode.Unauthorized);
    }
}

public class RecuperacaoSenhaTests(ApiFixture api) : ApiTestBase(api)
{
    private Task<HttpResponseMessage> Solicitar(string email) =>
        Anonimo.PostAsJsonAsync("/api/auth/forgot-password", new { email });

    private Task<HttpResponseMessage> Redefinir(string email, string codigo, string novaSenha = "Nova@12345") =>
        Anonimo.PostAsJsonAsync("/api/auth/reset-password", new { email, codigo, novaSenha });

    private Task<HttpResponseMessage> Login(string email, string senha) =>
        Anonimo.PostAsJsonAsync("/api/auth/login", new { email, senha });

    private static string Outro(string codigo) => codigo == "111111" ? "222222" : "111111";

    [Fact(DisplayName = "Solicitar recuperação envia um código de 6 dígitos")]
    public async Task EnviaCodigo()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        Assert.Matches(@"^\d{6}$", CodigoDeReset(conta.Email));
    }

    [Fact(DisplayName = "E-mail desconhecido responde igual e não envia nada (anti-enumeração)")]
    public async Task EmailDesconhecido()
    {
        var email = $"{Unico()}@teste.dev";
        await Sucesso(await Solicitar(email));
        Assert.DoesNotContain(Api.Email.Enviados, e => e.Para == email);
    }

    [Fact(DisplayName = "Solicitar recuperação sem e-mail é recusado")]
    public async Task SemEmail() => await Status(await Solicitar(""), HttpStatusCode.BadRequest);

    [Fact(DisplayName = "Fluxo completo: a nova senha passa a valer e a antiga deixa de valer")]
    public async Task FluxoCompleto()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        await Sucesso(await Redefinir(conta.Email, CodigoDeReset(conta.Email)));
        await Sucesso(await Login(conta.Email, "Nova@12345"));
        await Status(await Login(conta.Email, Senha), HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Código errado é recusado")]
    public async Task CodigoErrado()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        await Status(await Redefinir(conta.Email, Outro(CodigoDeReset(conta.Email))), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Código só pode ser usado uma vez")]
    public async Task CodigoUsoUnico()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        var codigo = CodigoDeReset(conta.Email);
        await Sucesso(await Redefinir(conta.Email, codigo));
        await Status(await Redefinir(conta.Email, codigo, "Outra@12345"), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Código expirado é recusado")]
    public async Task CodigoExpirado()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        await Api.Sql($"update usuarios set reset_senha_expiracao = now() - interval '1 minute' where email = {conta.Email}");
        await Status(await Redefinir(conta.Email, CodigoDeReset(conta.Email)), HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Nova senha fraca é recusada no reset")]
    public async Task SenhaFraca()
    {
        var conta = await NovaConta();
        await Sucesso(await Solicitar(conta.Email));
        await Status(await Redefinir(conta.Email, CodigoDeReset(conta.Email), "fraca"), HttpStatusCode.BadRequest);
    }
}
