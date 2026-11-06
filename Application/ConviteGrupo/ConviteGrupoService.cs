using System.Net;
using TribeWallet.Application.ConviteGrupo.DTOs;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Notificacoes;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;
using TribeWallet.Services;

namespace TribeWallet.Application.ConviteGrupo;

public class ConviteGrupoService
{
    /// <summary>Quanto tempo o link continua válido.</summary>
    private static readonly TimeSpan Validade = TimeSpan.FromDays(7);

    private readonly IConviteGrupoRepository _conviteRepository;
    private readonly Grupo.IGrupoRepository _grupoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IIntegranteRepository _integranteRepository;
    private readonly IEmailService _emailService;
    private readonly NotificacaoService _notificacaoService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConviteGrupoService> _logger;

    public ConviteGrupoService(IConviteGrupoRepository conviteRepository, Grupo.IGrupoRepository grupoRepository,
        IUsuarioRepository usuarioRepository, IIntegranteRepository integranteRepository,
        IEmailService emailService, NotificacaoService notificacaoService, IConfiguration configuration, ILogger<ConviteGrupoService> logger)
    {
        _conviteRepository = conviteRepository;
        _grupoRepository = grupoRepository;
        _usuarioRepository = usuarioRepository;
        _integranteRepository = integranteRepository;
        _emailService = emailService;
        _notificacaoService = notificacaoService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Gera o link de convite. Só quem já é integrante ativo do grupo pode convidar.</summary>
    public async Task<ConviteGrupoResponseDTO> Criar(string grupoToken, Guid usuarioId, CreateConviteGrupoRequestDTO dto)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);
        if (grupo is null || grupo.DeletedAt != null)
            throw new Exception("Grupo não encontrado.");

        var integrante = await _conviteRepository.GetIntegrante(grupo.GrupoId, usuarioId);
        if (integrante is null || integrante.DeletedAt != null)
            throw new UnauthorizedAccessException("Só integrantes do grupo podem gerar convites.");

        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        if (email != null && !System.Net.Mail.MailAddress.TryCreate(email, out _))
            throw new ArgumentException("E-mail inválido.");

        var convite = await _conviteRepository.Create(new Domain.Entities.ConviteGrupo
        {
            GrupoId = grupo.GrupoId,
            CriadoPorUsuarioId = usuarioId,
            EmailConvidado = email,
            ExpiraEm = DateTime.UtcNow.Add(Validade)
        });

        var link = MontarLink(convite.Token);
        var emailEnviado = false;
        if (email != null)
        {
            // Se o e-mail já é de um usuário, o convite aparece também no sininho dele.
            var convidado = await _usuarioRepository.GetByEmail(email);
            if (convidado != null)
                await _notificacaoService.Notificar([convidado.UsuarioId], TipoNotificacao.ConviteGrupo,
                    nameof(Domain.Entities.Grupo), grupo.GrupoId,
                    $"{integrante.Usuario!.Nome} convidou você para o grupo {grupo.Nome}.");

            try
            {
                var quem = WebUtility.HtmlEncode($"{integrante.Usuario!.Nome} {integrante.Usuario.Sobrenome}");
                var nomeGrupo = WebUtility.HtmlEncode(grupo.Nome);
                await _emailService.EnviarAsync(email, $"Convite para o grupo {grupo.Nome} no TribeWallet",
                    $"<p>{quem} convidou você para o grupo <b>{nomeGrupo}</b> no TribeWallet.</p>" +
                    $"<p><a href=\"{link}\">Entrar no grupo</a></p><p>{link}</p>" +
                    $"<p>O convite vale até {convite.ExpiraEm:dd/MM/yyyy}.</p>");
                emailEnviado = true;
            }
            catch (Exception e)
            {
                // O link já foi gerado e pode ser compartilhado à mão; o e-mail é só uma conveniência.
                _logger.LogWarning(e, "Não foi possível enviar o e-mail de convite.");
            }
        }

        return new ConviteGrupoResponseDTO
        {
            ConviteToken = convite.Token,
            Link = link,
            GrupoToken = grupo.Token,
            EmailConvidado = email,
            ExpiraEm = convite.ExpiraEm,
            EmailEnviado = emailEnviado
        };
    }

    /// <summary>Dados públicos do convite, para a tela "você foi convidado para o grupo X".</summary>
    public async Task<ConviteGrupoInfoDTO> Consultar(string conviteToken)
    {
        var convite = await ObterValido(conviteToken);
        return new ConviteGrupoInfoDTO
        {
            GrupoNome = convite.Grupo!.Nome,
            GrupoDescricao = convite.Grupo.Descricao,
            ConvidadoPor = $"{convite.CriadoPorUsuario!.Nome} {convite.CriadoPorUsuario.Sobrenome}",
            EmailConvidado = convite.EmailConvidado,
            ExpiraEm = convite.ExpiraEm
        };
    }

    /// <summary>Adiciona o usuário logado ao grupo do convite. Repetir a chamada não duplica o integrante.</summary>
    public async Task<string> Aceitar(string conviteToken, Guid usuarioId)
    {
        var convite = await ObterValido(conviteToken);

        var usuario = await _usuarioRepository.GetById(usuarioId)
                      ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        if (convite.EmailConvidado != null &&
            !string.Equals(convite.EmailConvidado, usuario.Email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Este convite foi enviado para outro e-mail.");

        var existente = await _conviteRepository.GetIntegrante(convite.GrupoId, usuario.UsuarioId);
        var entrouAgora = false;
        if (existente is null)
        {
            await _integranteRepository.Create(new Domain.Entities.Integrante
            {
                UsuarioId = usuario.UsuarioId,
                GrupoId = convite.GrupoId
            });
            entrouAgora = true;
        }
        else if (existente.DeletedAt != null)
        {
            // Quem saiu do grupo e volta por um convite reativa o vínculo antigo (índice único usuário+grupo).
            await _conviteRepository.RestaurarIntegrante(existente);
            entrouAgora = true;
        }

        // Só avisa quem convidou quando a pessoa realmente entrou; repetir a chamada não gera outro aviso.
        if (entrouAgora)
            await _notificacaoService.Notificar([convite.CriadoPorUsuarioId], TipoNotificacao.ConviteGrupo,
                nameof(Domain.Entities.Grupo), convite.GrupoId,
                $"{usuario.Nome} entrou no grupo {convite.Grupo!.Nome}.");

        return convite.Grupo!.Token;
    }

    private async Task<Domain.Entities.ConviteGrupo> ObterValido(string conviteToken)
    {
        var convite = await _conviteRepository.GetByToken(conviteToken)
                      ?? throw new Exception("Convite não encontrado.");

        if (convite.ExpiraEm < DateTime.UtcNow)
            throw new Exception("Este convite expirou.");
        if (convite.Grupo!.DeletedAt != null)
            throw new Exception("O grupo deste convite não existe mais.");

        return convite;
    }

    private string MontarLink(string token)
    {
        // Deep link do app (esquema "tribewallet" do app.json) por padrão; pode apontar para uma página web.
        var baseLink = (_configuration["CONVITE_LINK_BASE"] ?? "tribewallet://convite").TrimEnd('/');
        return $"{baseLink}/{token}";
    }
}
