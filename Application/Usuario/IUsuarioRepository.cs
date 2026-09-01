namespace TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

public interface IUsuarioRepository
{
    Task<Usuario> GetByToken(string token);
    Task<IEnumerable<Usuario>> GetAll();
    Task<Usuario> Create(Usuario usuario);
    Task<Usuario> Update(Usuario usuario);
    void Delete(int id);
    Task<Usuario> Login(LoginDTO loginDto);
}