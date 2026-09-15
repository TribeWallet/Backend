namespace TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

public interface IUsuarioRepository
{
    Task<Usuario> GetByToken(string token);
    Task<IEnumerable<Usuario>> GetByNome(string nome);
    Task<IEnumerable<Usuario>> GetAll(bool deleted);
    Task<Usuario> Create(Usuario usuario);
    Task<Usuario> Update(Usuario usuario);
    Task<Usuario> Delete(Usuario usuario);
    Task<Usuario> Login(LoginRequestDTO loginRequestDto);
}