namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;

public interface IGrupoRepository
{
    Task<Grupo> GetByToken(string token);
    Task<IEnumerable<Grupo>> GetByUsuarioToken(string usuarioToken);
    Task<Grupo> Create(Grupo grupo);
    Task<Grupo> Update(Grupo grupo);
    void Delete(int id);
}