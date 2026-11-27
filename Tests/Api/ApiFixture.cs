using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TribeWallet.Data;
using TribeWallet.Services;

// Um host por vez: evita duas instâncias migrando o mesmo banco de teste.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TribeWallet.Tests.Api;

/// <summary>
/// Isola TODA a suíte do banco de desenvolvimento. O .env é carregado com NoClobber,
/// então o que for definido aqui tem precedência.
/// </summary>
internal static class AmbienteDeTeste
{
    [ModuleInitializer]
    public static void Configurar()
    {
        var uploads = Path.Combine(Path.GetTempPath(), "tribewallet-test-uploads");
        Directory.CreateDirectory(uploads);
        Environment.SetEnvironmentVariable("POSTGRES_DB",
            Environment.GetEnvironmentVariable("TRIBEWALLET_TEST_DB") ?? "tribewallet_test");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", " ");
        Environment.SetEnvironmentVariable("APLICAR_MIGRATIONS_NO_STARTUP", "true");
        Environment.SetEnvironmentVariable("EXECUTAR_SEED", "false");
        Environment.SetEnvironmentVariable("STORAGE_LOCAL_PATH", uploads);
    }
}

public record EmailEnviado(string Para, string Assunto, string Corpo);

public class EmailFalso : IEmailService
{
    public ConcurrentQueue<EmailEnviado> Enviados { get; } = new();

    public Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
    {
        Enviados.Enqueue(new(destinatario, assunto, corpoHtml));
        return Task.CompletedTask;
    }
}

public class ApiFixture : WebApplicationFactory<Program>
{
    public EmailFalso Email { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) =>
        builder.ConfigureTestServices(s => s.AddSingleton<IEmailService>(Email));

    public async Task Sql(FormattableString sql)
    {
        using var escopo = Services.CreateScope();
        await escopo.ServiceProvider.GetRequiredService<AppDbContext>().Database.ExecuteSqlInterpolatedAsync(sql);
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture> { }

public record Conta(string Token, string Email, string Username, string Jwt, HttpClient Http);

public record Cenario(Conta Dono, Conta Membro, string Grupo, string IntegranteDono, string IntegranteMembro);

[Collection("Api")]
public abstract class ApiTestBase
{
    protected const string Senha = "Senha@123";
    protected readonly ApiFixture Api;
    protected readonly HttpClient Anonimo;

    protected ApiTestBase(ApiFixture api)
    {
        Api = api;
        Anonimo = api.CreateClient();
    }

    protected static string Unico() => Guid.NewGuid().ToString("N")[..12];

    protected static object Cadastro(string id, string senha = Senha) => new
    {
        nome = "Nome" + id, sobrenome = "Teste", email = $"{id}@teste.dev", username = "u" + id, senha
    };

    protected async Task<Conta> NovaConta()
    {
        var id = Unico();
        var registro = await Json(await Anonimo.PostAsJsonAsync("/api/auth/register", Cadastro(id)));
        var login = await Json(await Anonimo.PostAsJsonAsync("/api/auth/login", new { email = $"{id}@teste.dev", senha = Senha }));
        var jwt = login.GetProperty("jwtToken").GetString()!;
        var http = Api.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return new Conta(registro.GetProperty("usuarioToken").GetString()!, $"{id}@teste.dev", "u" + id, jwt, http);
    }

    protected async Task<Cenario> NovoCenario()
    {
        var dono = await NovaConta();
        var membro = await NovaConta();
        var grupo = await Json(await dono.Http.PostAsJsonAsync("/api/grupos", new
        {
            nome = "Grupo " + Unico(), descricao = "teste",
            integrantes = new[] { new { usuarioToken = dono.Token }, new { usuarioToken = membro.Token } }
        }));
        string Integrante(string usuario) => grupo.GetProperty("integrantes").EnumerateArray()
            .Single(i => i.GetProperty("usuario").GetProperty("usuarioToken").GetString() == usuario)
            .GetProperty("integranteToken").GetString()!;
        return new Cenario(dono, membro, grupo.GetProperty("grupoToken").GetString()!,
            Integrante(dono.Token), Integrante(membro.Token));
    }

    protected static MultipartFormDataContent Form(params (string, string)[] campos)
    {
        var form = new MultipartFormDataContent();
        foreach (var (nome, valor) in campos) form.Add(new StringContent(valor), nome);
        return form;
    }

    protected static MultipartFormDataContent ComArquivo(MultipartFormDataContent form, string campo,
        string nome = "comprovante.png", string tipo = "image/png", string conteudo = "conteudo-de-teste")
    {
        var arquivo = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(conteudo));
        arquivo.Headers.ContentType = new MediaTypeHeaderValue(tipo);
        form.Add(arquivo, campo, nome);
        return form;
    }

    protected static string Data(int dias) => DateTime.UtcNow.AddDays(dias).ToString("O");

    protected static MultipartFormDataContent FormCompromisso(string valor = "100.00", int dias = 10,
        int divisao = 1, string titulo = "Mercado") => Form(("Titulo", titulo), ("ValorTotal", valor),
        ("Data", Data(dias)), ("TipoDivisao", divisao.ToString()), ("Categoria", "Alimentacao"));

    protected async Task<string> NovoCompromisso(Cenario c, string valor = "100.00", int dias = 10, int divisao = 1)
    {
        var r = await Json(await c.Dono.Http.PostAsync($"/api/compromissos/{c.Grupo}", FormCompromisso(valor, dias, divisao)));
        return r.GetProperty("compromissoFinanceiroToken").GetString()!;
    }

    protected static object Parte(string integrante, decimal devedor) =>
        new { integranteToken = integrante, valorDevedor = devedor, valorPago = 0m };

    /// <summary>Compromisso de 100 com o membro devendo 50. Devolve (compromisso, fatia).</summary>
    protected async Task<(string Compromisso, string Fatia)> NovaFatia(Cenario c, decimal devedor = 50m, int dias = 10)
    {
        var compromisso = await NovoCompromisso(c, dias: dias);
        var r = await Json(await c.Dono.Http.PostAsJsonAsync($"/api/compromissos/{compromisso}/integrantes",
            new[] { Parte(c.IntegranteMembro, devedor) }));
        return (compromisso, r.GetProperty("participacoes")[0].GetProperty("integranteCompromissoToken").GetString()!);
    }

    protected static Task<HttpResponseMessage> Pagar(HttpClient http, string fatia, string valor, int metodo = 1) =>
        http.PostAsync("/api/Pagamento", ComArquivo(Form(("IntegranteCompromissoToken", fatia), ("Valor", valor),
            ("Data", Data(0)), ("Metodo", metodo.ToString())), "Comprovante"));

    protected async Task<string> NovoPagamento(Conta quem, string fatia, string valor)
    {
        var r = await Json(await Pagar(quem.Http, fatia, valor));
        return r.GetProperty("pagamentoToken").GetString()!;
    }

    protected static async Task<JsonElement> Json(HttpResponseMessage r)
    {
        var corpo = await r.Content.ReadAsStringAsync();
        Assert.True(r.IsSuccessStatusCode, $"Esperava sucesso, veio {(int)r.StatusCode}: {Cortar(corpo)}");
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    protected static async Task Sucesso(HttpResponseMessage r) =>
        Assert.True(r.IsSuccessStatusCode, $"Esperava sucesso, veio {(int)r.StatusCode}: {Cortar(await r.Content.ReadAsStringAsync())}");

    protected static async Task Status(HttpResponseMessage r, params HttpStatusCode[] esperados) =>
        Assert.True(esperados.Contains(r.StatusCode),
            $"Esperava {string.Join(" ou ", esperados.Select(e => (int)e))}, veio {(int)r.StatusCode}: {Cortar(await r.Content.ReadAsStringAsync())}");

    protected static Task Recusado(HttpResponseMessage r) => Status(r, HttpStatusCode.BadRequest,
        HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.Conflict, HttpStatusCode.Unauthorized);

    private static string Cortar(string s) => s.Length > 300 ? s[..300] + "…" : s;

    protected async Task<JsonElement> Compromisso(Cenario c, string token)
    {
        var lista = await Json(await c.Dono.Http.GetAsync($"/api/compromissos/{c.Grupo}?deleted=false"));
        return lista.EnumerateArray().Single(x => x.GetProperty("compromissoFinanceiroToken").GetString() == token);
    }

    protected static async Task<List<JsonElement>> Notificacoes(Conta quem) =>
        (await Json(await quem.Http.GetAsync("/api/notificacoes"))).EnumerateArray().ToList();

    protected static int Tipo(JsonElement n) => n.GetProperty("tipo").GetInt32();

    protected string CodigoDeReset(string email)
    {
        var mensagem = Api.Email.Enviados.Last(e => e.Para == email);
        return Regex.Match(mensagem.Corpo, @"<b>(\d{6})</b>").Groups[1].Value;
    }
}
