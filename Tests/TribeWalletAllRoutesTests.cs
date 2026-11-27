namespace TribeWallet.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

[CollectionDefinition("TribeWalletRouteCollection", DisableParallelization = true)]
public class TribeWalletRouteCollection { }

[Collection("TribeWalletRouteCollection")]
[TestCaseOrderer("TribeWallet.Tests.PriorityOrderer", "TribeWallet.Tests")]
public class TribeWalletAllRoutesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    // Shared state populated across the ordered flow
    private static readonly string RunId = Guid.NewGuid().ToString("N")[..8];
    private static readonly string PrimaryEmail = $"user1_{RunId}@test.com";
    private static readonly string SecondaryEmail = $"user2_{RunId}@test.com";
    private const string Password = "Test@123456";

    private static string? _jwtToken;
    private static string? _usuarioToken;
    private static string? _secondaryUsuarioToken;
    private static string? _grupoToken;
    private static string? _conviteToken;
    private static string? _integranteToken;
    private static string? _compromissoToken;
    private static string? _integranteCompromissoToken;
    private static string? _pagamentoToken;

    public TribeWalletAllRoutesTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        if (!string.IsNullOrEmpty(_jwtToken))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }
    }

    // ==================== 1. AUTH ====================

    [Fact, TestPriority(1)]
    public async Task T01_Post_AuthRegister()
    {
        var res1 = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            nome = "Teste",
            sobrenome = "Um",
            email = PrimaryEmail,
            username = $"user1_{RunId}",
            senha = Password
        });
        await AssertSuccessAsync(res1, "POST /api/auth/register (primary)");

        var res2 = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            nome = "Teste",
            sobrenome = "Dois",
            email = SecondaryEmail,
            username = $"user2_{RunId}",
            senha = Password
        });
        await AssertSuccessAsync(res2, "POST /api/auth/register (secondary)");
    }

    [Fact, TestPriority(2)]
    public async Task T02_Post_AuthLogin()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = PrimaryEmail,
            senha = Password
        });
        await AssertSuccessAsync(res, "POST /api/auth/login");

        var rawBody = await res.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(rawBody).RootElement;
        _jwtToken = ExtractToken(json, "token", "accessToken", "jwt", "jwtToken", "bearerToken");
        _usuarioToken = ExtractToken(json, "usuarioToken", "tokenUsuario", "userToken", "id");
        Assert.False(string.IsNullOrWhiteSpace(_jwtToken),
            $"Login response did not contain a JWT token. Response body: {rawBody}");
    }

    [Fact, TestPriority(3)]
    public async Task T03_Post_AuthForgotPassword()
    {
        var res = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = SecondaryEmail,
            novaSenha = "NewPassword@123"
        });
        await AssertSuccessAsync(res, "POST /api/auth/forgot-password");
    }

    // ==================== 2. USUARIOS ====================

    [Fact, TestPriority(4)]
    public async Task T04_Get_Usuarios()
    {
        var res = await _client.GetAsync("/api/usuarios?deleted=false");
        await AssertSuccessAsync(res, "GET /api/usuarios");

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var user in json.EnumerateArray())
            {
                var email = TryGetString(user, "email");
                var token = TryGetString(user, "token", "usuarioToken");
                if (email == PrimaryEmail && string.IsNullOrEmpty(_usuarioToken)) _usuarioToken = token;
                if (email == SecondaryEmail) _secondaryUsuarioToken = token;
            }
        }
        Assert.False(string.IsNullOrWhiteSpace(_usuarioToken), "Could not resolve usuarioToken.");
    }

    [Fact, TestPriority(5)]
    public async Task T05_Get_UsuarioByNome()
    {
        var res = await _client.GetAsync("/api/usuarios/Teste");
        await AssertSuccessAsync(res, "GET /api/usuarios/{nome}");
    }

    [Fact, TestPriority(6)]
    public async Task T06_Put_Usuario()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("TesteAtualizado"), "Nome");
        form.Add(new StringContent("Um"), "Sobrenome");
        form.Add(new StringContent($"user1_{RunId}"), "Username");

        var res = await _client.PutAsync($"/api/usuarios/{_usuarioToken}", form);
        await AssertSuccessAsync(res, "PUT /api/usuarios/{usuarioToken}");
    }

    // ==================== 3. GRUPOS & CONVITES ====================

    [Fact, TestPriority(7)]
    public async Task T07_Post_Grupos()
    {
        var integrantes = new List<object>();
        if (!string.IsNullOrEmpty(_usuarioToken))
        {
            integrantes.Add(new { usuarioToken = _usuarioToken });
        }
        if (!string.IsNullOrEmpty(_secondaryUsuarioToken))
        {
            integrantes.Add(new { usuarioToken = _secondaryUsuarioToken });
        }

        var payload = new
        {
            nome = $"Viagem_{RunId}",
            descricao = "Grupo de teste automatizado",
            integrantes
        };

        var res = await _client.PostAsJsonAsync("/api/grupos", payload);
        await AssertSuccessAsync(res, "POST /api/grupos");

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        _grupoToken = ExtractToken(json, "grupoToken", "token");
    }

    [Fact, TestPriority(8)]
    public async Task T08_Get_GruposByUsuario()
    {
        var res = await _client.GetAsync($"/api/grupos/usuario/{_usuarioToken}?deleted=false");
        await AssertSuccessAsync(res, "GET /api/grupos/usuario/{usuarioToken}");

        if (string.IsNullOrEmpty(_grupoToken))
        {
            var json = await res.Content.ReadFromJsonAsync<JsonElement>();
            _grupoToken = ExtractToken(json, "grupoToken", "token");
        }
        Assert.False(string.IsNullOrWhiteSpace(_grupoToken), "Could not resolve grupoToken.");
    }

    [Fact, TestPriority(9)]
    public async Task T09_Get_GrupoByToken()
    {
        var res = await _client.GetAsync($"/api/grupos/{_grupoToken}?deleted=false");
        await AssertSuccessAsync(res, "GET /api/grupos/{grupoToken}");

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        _integranteToken = ExtractToken(json, "integranteToken");
    }

    [Fact, TestPriority(10)]
    public async Task T10_Put_Grupo()
    {
        var res = await _client.PutAsJsonAsync($"/api/grupos/{_grupoToken}", new
        {
            nome = $"ViagemAtualizada_{RunId}",
            descricao = "Descricao atualizada"
        });
        await AssertSuccessAsync(res, "PUT /api/grupos/{grupoToken}");
    }

    [Fact, TestPriority(11)]
    public async Task T11_Put_GrupoIntegrantes()
    {
        var novo = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            nome = "Teste", sobrenome = "Tres", email = $"user3_{RunId}@test.com",
            username = $"user3_{RunId}", senha = Password
        });
        await AssertSuccessAsync(novo, "POST /api/auth/register (third)");
        var terceiro = (await novo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("usuarioToken").GetString();

        var res = await _client.PutAsJsonAsync($"/api/grupos/{_grupoToken}/integrantes", new[] { new { usuarioToken = terceiro } });
        await AssertSuccessAsync(res, "PUT /api/grupos/{grupoToken}/integrantes");
    }

    [Fact, TestPriority(12)]
    public async Task T12_Post_GrupoConvites()
    {
        var payload = new
        {
            usuarioToken = _secondaryUsuarioToken
        };

        var res = await _client.PostAsJsonAsync($"/api/grupos/{_grupoToken}/convites", payload);
        await AssertSuccessAsync(res, "POST /api/grupos/{grupoToken}/convites");

        var body = (await res.Content.ReadAsStringAsync()).Trim();
        Assert.False(string.IsNullOrWhiteSpace(body), "Expected a response body from POST /api/grupos/{grupoToken}/convites.");

        if (body.StartsWith("{") || body.StartsWith("["))
        {
            using var doc = JsonDocument.Parse(body);
            var extracted = ExtractToken(doc.RootElement, "conviteToken", "token", "codigo", "id");
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                _conviteToken = extracted;
            }
        }
    }

    [Fact, TestPriority(13)]
    public async Task T13_Get_Convite()
    {
        Assert.False(string.IsNullOrEmpty(_conviteToken), "Pré-condição ausente: _conviteToken não foi obtido nos testes anteriores.");
        var res = await _client.GetAsync($"/api/convites/{_conviteToken}");
        await AssertSuccessAsync(res, "GET /api/convites/{conviteToken}");
    }

    [Fact, TestPriority(14)]
    public async Task T14_Post_AceitarConvite()
    {
        Assert.False(string.IsNullOrEmpty(_conviteToken), "Pré-condição ausente: _conviteToken não foi obtido nos testes anteriores.");
        var res = await _client.PostAsync($"/api/convites/{_conviteToken}/aceitar", null);
        await AssertSuccessAsync(res, "POST /api/convites/{conviteToken}/aceitar");
    }

    // ==================== 4. COMPROMISSOS ====================

    [Fact, TestPriority(15)]
    public async Task T15_Post_Compromissos()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Jantar Teste"), "Titulo");
        form.Add(new StringContent("150.00"), "ValorTotal");
        form.Add(new StringContent("50"), "Percentual");
        form.Add(new StringContent(DateTime.UtcNow.ToString("O")), "Data");
        form.Add(new StringContent("1"), "TipoDivisao");
        form.Add(new StringContent("Alimentacao"), "Categoria");

        var res = await _client.PostAsync($"/api/compromissos/{_grupoToken}", form);
        await AssertSuccessAsync(res, $"POST /api/compromissos/{_grupoToken}");

        var rawBody = await res.Content.ReadAsStringAsync();
        var trimmed = rawBody.Trim();

        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
        {
            using var doc = JsonDocument.Parse(trimmed);
            _compromissoToken = ExtractToken(doc.RootElement, "compromissoFinanceiroToken", "compromissoToken", "token", "codigo", "id");        }
        else if (!string.IsNullOrWhiteSpace(trimmed))
        {
            _compromissoToken = trimmed.Trim('"');
        }

        Assert.False(string.IsNullOrWhiteSpace(_compromissoToken),
            $"Could not extract _compromissoToken from T15 response. Status: {(int)res.StatusCode}, Body: {rawBody}");
        
    }

    [Fact, TestPriority(16)]
    public async Task T16_Get_CompromissosByGrupo()
    {
        var res = await _client.GetAsync($"/api/compromissos/{_grupoToken}?deleted=false");
        await AssertSuccessAsync(res, "GET /api/compromissos/{grupoToken}");

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        if (string.IsNullOrEmpty(_compromissoToken))
            _compromissoToken = ExtractToken(json, "compromissoToken", "token");
        if (string.IsNullOrEmpty(_integranteCompromissoToken))
            _integranteCompromissoToken = ExtractToken(json, "integranteCompromissoToken");
    }

    [Fact, TestPriority(17)]
    public async Task T17_Get_CompromissosByUsuario()
    {
        var res = await _client.GetAsync($"/api/compromissos/usuario/{_usuarioToken}?deleted=false");
        await AssertSuccessAsync(res, "GET /api/compromissos/usuario/{usuarioToken}");
    }

    [Fact, TestPriority(18)]
    public async Task T18_Get_CompromissosByIntegrante()
    {
        Assert.False(string.IsNullOrEmpty(_integranteToken), "Pré-condição ausente: _integranteToken não foi obtido nos testes anteriores.");
        var res = await _client.GetAsync($"/api/compromissos/integrantes/{_integranteToken}");
        await AssertSuccessAsync(res, "GET /api/compromissos/integrantes/{integranteToken}");
    }

    [Fact, TestPriority(19)]
    public async Task T19_Post_CompromissoIntegrantes()
    {
        Assert.False(string.IsNullOrWhiteSpace(_compromissoToken), "_compromissoToken was null or empty before T19.");

        var payload = new[]
        {
            new
            {
                integranteToken = _integranteToken,
                usuarioToken = _secondaryUsuarioToken,
                valorDevedor = 50.00m
            }
        };

        var res = await _client.PostAsJsonAsync($"/api/compromissos/{_compromissoToken}/integrantes", payload);
        await AssertSuccessAsync(res, $"POST /api/compromissos/{_compromissoToken}/integrantes");

        var body = (await res.Content.ReadAsStringAsync()).Trim();
        if (body.StartsWith("{") || body.StartsWith("["))
        {
            using var doc = JsonDocument.Parse(body);
            var extracted = ExtractToken(doc.RootElement, "integranteCompromissoToken");
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                _integranteCompromissoToken = extracted;
            }
        }
        else if (!string.IsNullOrWhiteSpace(body))
        {
            _integranteCompromissoToken = body.Trim('"');
        }
    }

    [Fact, TestPriority(20)]
    public async Task T20_Put_Compromisso()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Jantar Atualizado"), "Titulo");
        form.Add(new StringContent("200.00"), "ValorTotal");
        form.Add(new StringContent(DateTime.UtcNow.ToString("O")), "Data");
        form.Add(new StringContent("1"), "TipoDivisao");
        form.Add(new StringContent("Lazer"), "Categoria");

        var res = await _client.PutAsync($"/api/compromissos/{_compromissoToken}", form);
        await AssertSuccessAsync(res, "PUT /api/compromissos/{compromissoToken}");
    }

    // ==================== 5. PAGAMENTOS ====================

    [Fact, TestPriority(21)]
    public async Task T21_Post_Pagamento()
    {
        Assert.False(string.IsNullOrEmpty(_integranteCompromissoToken), "Pré-condição ausente: _integranteCompromissoToken não foi obtido nos testes anteriores.");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(_integranteCompromissoToken), "IntegranteCompromissoToken");
        form.Add(new StringContent("50.00"), "Valor");
        form.Add(new StringContent(DateTime.UtcNow.ToString("O")), "Data");
        form.Add(new StringContent("1"), "Metodo");

        var arquivo = new ByteArrayContent(new byte[] { 1, 2, 3 });
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(arquivo, "Comprovante", "comprovante.png");

        var res = await _client.PostAsync("/api/Pagamento", form);
        await AssertSuccessAsync(res, "POST /api/Pagamento");

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        _pagamentoToken = ExtractToken(json, "pagamentoToken", "token");
    }

    [Fact, TestPriority(22)]
    public async Task T22_Put_Pagamento()
    {
        Assert.False(string.IsNullOrEmpty(_pagamentoToken), "Pré-condição ausente: _pagamentoToken não foi obtido nos testes anteriores.");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("75.00"), "Valor");
        form.Add(new StringContent(DateTime.UtcNow.ToString("O")), "Data");
        form.Add(new StringContent("2"), "Metodo");

        var arquivo = new ByteArrayContent(new byte[] { 1, 2, 3 });
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(arquivo, "Comprovante", "comprovante.png");

        var res = await _client.PutAsync($"/api/Pagamento/{_pagamentoToken}", form);
        await AssertSuccessAsync(res, "PUT /api/Pagamento/{token}");
    }

    // ==================== 6. RELATORIOS & DASHBOARD ====================

    [Fact, TestPriority(23)]
    public async Task T23_Get_UsuarioDashboard()
    {
        var res = await _client.GetAsync($"/api/usuarios/{_usuarioToken}/dashboard");
        await AssertSuccessAsync(res, "GET /api/usuarios/{usuarioToken}/dashboard");
    }

    [Fact, TestPriority(24)]
    public async Task T24_Get_RelatorioUsuario()
    {
        var res = await _client.GetAsync($"/api/relatorios/usuario/{_usuarioToken}");
        await AssertSuccessAsync(res, "GET /api/relatorios/usuario/{usuarioToken}");
    }

    [Fact, TestPriority(25)]
    public async Task T25_Get_RelatorioUsuarioPdf()
    {
        var res = await _client.GetAsync($"/api/relatorios/usuario/{_usuarioToken}/pdf");
        await AssertSuccessAsync(res, "GET /api/relatorios/usuario/{usuarioToken}/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact, TestPriority(26)]
    public async Task T26_Get_RelatorioUsuarioGrupo()
    {
        var res = await _client.GetAsync($"/api/relatorios/usuario/{_usuarioToken}/grupo/{_grupoToken}");
        await AssertSuccessAsync(res, "GET /api/relatorios/usuario/{usuarioToken}/grupo/{grupoToken}");
    }

    [Fact, TestPriority(27)]
    public async Task T27_Get_RelatorioUsuarioGrupoPdf()
    {
        var res = await _client.GetAsync($"/api/relatorios/usuario/{_usuarioToken}/grupo/{_grupoToken}/pdf");
        await AssertSuccessAsync(res, "GET /api/relatorios/usuario/{usuarioToken}/grupo/{grupoToken}/pdf");
        var bytes = await res.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    // ==================== 7. DELETES (CLEANUP ORDER) ====================

    [Fact, TestPriority(28)]
    public async Task T28_Delete_Pagamento()
    {
        Assert.False(string.IsNullOrEmpty(_pagamentoToken), "Pré-condição ausente: _pagamentoToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/Pagamento/{_pagamentoToken}");
        await AssertSuccessAsync(res, "DELETE /api/Pagamento/{token}");
    }

    [Fact, TestPriority(29)]
    public async Task T29_Delete_CompromissoIntegrante()
    {
        Assert.False(string.IsNullOrEmpty(_integranteCompromissoToken), "Pré-condição ausente: _integranteCompromissoToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/compromissos/integrantes/{_integranteCompromissoToken}");
        await AssertSuccessAsync(res, "DELETE /api/compromissos/integrantes/{integranteCompromissoToken}");
    }

    [Fact, TestPriority(30)]
    public async Task T30_Delete_Compromisso()
    {
        Assert.False(string.IsNullOrEmpty(_compromissoToken), "Pré-condição ausente: _compromissoToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/compromissos/{_compromissoToken}");
        await AssertSuccessAsync(res, "DELETE /api/compromissos/{compromissoToken}");
    }

    [Fact, TestPriority(31)]
    public async Task T31_Delete_GrupoIntegrante()
    {
        Assert.False(string.IsNullOrEmpty(_integranteToken), "Pré-condição ausente: _integranteToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/grupos/{_grupoToken}/integrantes/{_integranteToken}");
        await AssertSuccessAsync(res, "DELETE /api/grupos/{grupoToken}/integrantes/{integranteToken}");
    }

    [Fact, TestPriority(32)]
    public async Task T32_Delete_Grupo()
    {
        Assert.False(string.IsNullOrEmpty(_grupoToken), "Pré-condição ausente: _grupoToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/grupos/{_grupoToken}");
        await AssertSuccessAsync(res, "DELETE /api/grupos/{grupoToken}");
    }

    [Fact, TestPriority(33)]
    public async Task T33_Delete_Usuario()
    {
        Assert.False(string.IsNullOrEmpty(_usuarioToken), "Pré-condição ausente: _usuarioToken não foi obtido nos testes anteriores.");
        var res = await _client.DeleteAsync($"/api/usuarios/{_usuarioToken}");
        await AssertSuccessAsync(res, "DELETE /api/usuarios/{usuarioToken}");
    }

    // ==================== HELPERS ====================

    private static async Task AssertSuccessAsync(HttpResponseMessage res, string routeLabel)
    {
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            Assert.Fail($"{routeLabel} failed with {(int)res.StatusCode} ({res.StatusCode}). Body: {body}");
        }
    }

    private static string? ExtractToken(JsonElement element, params string[] candidateKeys)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (candidateKeys.Any(k => prop.Name.Equals(k, StringComparison.OrdinalIgnoreCase))
                    && prop.Value.ValueKind == JsonValueKind.String)
                {
                    return prop.Value.GetString();
                }

                var nested = ExtractToken(prop.Value, candidateKeys);
                if (!string.IsNullOrEmpty(nested)) return nested;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = ExtractToken(item, candidateKeys);
                if (!string.IsNullOrEmpty(nested)) return nested;
            }
        }
        return null;
    }

    private static string? TryGetString(JsonElement obj, params string[] keys)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            if (keys.Any(k => prop.Name.Equals(k, StringComparison.OrdinalIgnoreCase))
                && prop.Value.ValueKind == JsonValueKind.String)
            {
                return prop.Value.GetString();
            }
        }
        return null;
    }
}