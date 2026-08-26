using TribeWallet.Application;
using TribeWallet.Domain;

namespace TribeWallet.Infrastructure;

public class UsuarioRepositoryImplementation : IUsuarioRepository
{
    public Usuario GetById(int id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Usuario> GetAll()
    {
        throw new NotImplementedException();
    }

    public void Add(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    public void Update(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Usuario Login(LoginDTO loginDto)
    {
        //logica de login

        return GetById(1);
    }
}