namespace TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

public interface IUsuarioRepository
{
    Usuario GetById(Guid id);
    Usuario GetByToken(string token);
    IEnumerable<Usuario> GetAll();
    Usuario Create(Usuario usuario);
    Usuario Update(Usuario usuario);
    void Delete(int id);
    Usuario Login(LoginDTO loginDto);
}