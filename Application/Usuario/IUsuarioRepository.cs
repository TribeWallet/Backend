namespace TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

public interface IUsuarioRepository
{
    Usuario GetById(Guid id);
    IEnumerable<Usuario> GetAll();
    void Add(Usuario usuario);
    void Update(Usuario usuario);
    void Delete(int id);
    Usuario Login(LoginDTO loginDto);
}