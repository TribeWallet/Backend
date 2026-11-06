using System.Globalization;
using System.Security.Claims;
using TribeWallet.Application.Notificacoes.DTOs;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Notificacoes;

/// <summary>
/// Guarda e entrega os avisos. Sem FCM, o app só descobre o que há de novo perguntando à API;
/// por isso a listagem também gera as cobranças de vencimento, que não nascem de uma ação.
/// </summary>
public class NotificacaoService
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>Quantos dias antes do vencimento a cobrança passa a aparecer.</summary>
    private const int JanelaCobrancaDias = 3;

    private const int LimiteListagem = 100;

    private readonly INotificacaoRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(INotificacaoRepository repository, IUsuarioRepository usuarioRepository,
        IHttpContextAccessor httpContextAccessor, ILogger<NotificacaoService> logger)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>Usuário da requisição em andamento; nulo fora de uma requisição autenticada.</summary>
    public Guid? UsuarioAtualId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    /// <summary>Nome de quem está executando a ação, para montar a mensagem do aviso.</summary>
    public async Task<string> NomeDoAtor()
    {
        var id = UsuarioAtualId;
        var usuario = id is null ? null : await _usuarioRepository.GetById(id.Value);
        return usuario is null ? "Alguém" : usuario.Nome;
    }

    /// <summary>
    /// Cria um aviso para cada destinatário. Quem executou a ação nunca recebe o próprio aviso.
    /// Uma falha aqui é só registrada: a operação que gerou o aviso já foi concluída e não deve falhar por isso.
    /// </summary>
    public async Task Notificar(IEnumerable<Guid> destinatarios, TipoNotificacao tipo, string entidade,
        Guid entidadeId, string mensagem, bool doSistema = false)
    {
        try
        {
            var origem = doSistema ? null : UsuarioAtualId;
            var agora = DateTime.UtcNow;

            var avisos = destinatarios
                .Distinct()
                .Where(d => d != origem)
                .Select(d => new Notificacao
                {
                    UsuarioDestinoId = d,
                    UsuarioOrigemId = origem,
                    Entidade = entidade,
                    EntidadeId = entidadeId,
                    Tipo = tipo,
                    Mensagem = mensagem.Length > 500 ? mensagem[..500] : mensagem,
                    DataEnvio = agora
                })
                .ToList();

            if (avisos.Count > 0)
                await _repository.Add(avisos);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Não foi possível gerar a notificação {Tipo}.", tipo);
        }
    }

    public async Task<List<NotificacaoResponseDTO>> Listar(Guid usuarioId)
    {
        await GerarCobrancas(usuarioId);

        var notificacoes = await _repository.ListarPorUsuario(usuarioId, LimiteListagem);
        var tokens = await _repository.ResolverTokens(notificacoes);

        return notificacoes.Select(n => new NotificacaoResponseDTO
        {
            NotificacaoToken = n.Token,
            Tipo = n.Tipo,
            Mensagem = n.Mensagem,
            Entidade = n.Entidade,
            EntidadeToken = tokens.GetValueOrDefault(n.EntidadeId),
            OrigemNome = n.UsuarioOrigem is null ? null : $"{n.UsuarioOrigem.Nome} {n.UsuarioOrigem.Sobrenome}".Trim(),
            DataEnvio = n.DataEnvio,
            Lida = n.Lida
        }).ToList();
    }

    public async Task<NotificacaoContagemDTO> ContarNaoLidas(Guid usuarioId)
    {
        // A contagem também precisa enxergar as cobranças, senão o badge e a lista discordam.
        await GerarCobrancas(usuarioId);
        return new NotificacaoContagemDTO { NaoLidas = await _repository.ContarNaoLidas(usuarioId) };
    }

    public async Task MarcarComoLida(string token, Guid usuarioId)
    {
        var notificacao = await ObterDoUsuario(token, usuarioId);
        if (notificacao.Lida)
            return;

        notificacao.Lida = true;
        await _repository.Update(notificacao);
    }

    public Task MarcarTodasComoLidas(Guid usuarioId) => _repository.MarcarTodasComoLidas(usuarioId);

    public async Task Excluir(string token, Guid usuarioId)
    {
        var notificacao = await ObterDoUsuario(token, usuarioId);
        notificacao.DeletedAt = DateTime.UtcNow;
        await _repository.Update(notificacao);
    }

    /// <summary>Notificação de outro usuário responde como inexistente, para não confirmar que o token existe.</summary>
    private async Task<Notificacao> ObterDoUsuario(string token, Guid usuarioId)
    {
        var notificacao = await _repository.GetByToken(token);
        if (notificacao is null || notificacao.UsuarioDestinoId != usuarioId)
            throw new KeyNotFoundException("Notificação não encontrada.");

        return notificacao;
    }

    /// <summary>
    /// Cria o aviso de quem ainda deve algo que vence em breve (ou já venceu). No máximo um por
    /// compromisso por dia, para abrir o sininho várias vezes não encher a lista.
    /// </summary>
    private async Task GerarCobrancas(Guid usuarioId)
    {
        try
        {
            var hoje = DateTime.UtcNow.Date;
            var fatias = await _repository.GetFatiasEmAberto(usuarioId, hoje.AddDays(JanelaCobrancaDias + 1));
            if (fatias.Count == 0)
                return;

            var jaEnviadas = await _repository.CobrancasJaEnviadas(
                usuarioId, fatias.Select(f => f.CompromissoId), hoje);

            var novas = fatias
                .Where(f => !jaEnviadas.Contains(f.CompromissoId))
                .Select(f => new Notificacao
                {
                    UsuarioDestinoId = usuarioId,
                    UsuarioOrigemId = null,
                    Entidade = nameof(CompromissoFinanceiro),
                    EntidadeId = f.CompromissoId,
                    Tipo = TipoNotificacao.CobrancaPendente,
                    Mensagem = MensagemCobranca(f, hoje),
                    DataEnvio = DateTime.UtcNow
                })
                .ToList();

            if (novas.Count > 0)
                await _repository.Add(novas);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Não foi possível gerar as cobranças do usuário.");
        }
    }

    private static string MensagemCobranca(Domain.Entities.IntegranteCompromisso fatia, DateTime hoje)
    {
        var titulo = fatia.Compromisso!.Titulo;
        var restante = Moeda(fatia.ValorDevedor - fatia.ValorPago);
        var dias = (fatia.Compromisso.Data.Date - hoje).Days;

        var quando = dias switch
        {
            < 0 => $"venceu em {fatia.Compromisso.Data:dd/MM}",
            0 => "vence hoje",
            1 => "vence amanhã",
            _ => $"vence em {dias} dias"
        };

        return $"{titulo} {quando}. Sua parte em aberto: {restante}.";
    }
}
