using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Notificacoes;

public interface INotificacaoRepository
{
    Task Add(IEnumerable<Notificacao> notificacoes);

    /// <summary>Mais recentes primeiro, sem as excluídas.</summary>
    Task<List<Notificacao>> ListarPorUsuario(Guid usuarioId, int limite);

    Task<int> ContarNaoLidas(Guid usuarioId);

    Task<Notificacao?> GetByToken(string token);

    Task Update(Notificacao notificacao);

    Task MarcarTodasComoLidas(Guid usuarioId);

    /// <summary>Fatias do usuário ainda não quitadas, de compromissos que vencem até a data informada.</summary>
    Task<List<Domain.Entities.IntegranteCompromisso>> GetFatiasEmAberto(Guid usuarioId, DateTime ate);

    /// <summary>Compromissos que já geraram cobrança para o usuário desde o instante informado.</summary>
    Task<HashSet<Guid>> CobrancasJaEnviadas(Guid usuarioId, IEnumerable<Guid> compromissoIds, DateTime desde);

    /// <summary>Id interno do registro de origem → token público, para o app não ver UUIDs.</summary>
    Task<Dictionary<Guid, string>> ResolverTokens(IEnumerable<Notificacao> notificacoes);
}
