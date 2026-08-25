namespace TribeWallet.Application;
using TribeWallet.Domain;

public interface IUsuarioRepository
{
    Usuario GetById(int id);
    IEnumerable<Usuario> GetAll();
    void Add(Usuario usuario);
    void Update(Usuario usuario);
    void Delete(int id);
    Usuario Login(LoginDTO loginDto);
}