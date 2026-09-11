namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;

public interface IGrupoRepository
{
    Task<Grupo> GetByToken(string token);
    Task<IEnumerable<Grupo>> GetAllByUsuarioToken(string usuarioToken, bool deleted);
    Task<Grupo> Create(Grupo grupo);
    Task<Grupo> Update(Grupo grupo);
    Task<Grupo> Delete(Grupo grupo);
}