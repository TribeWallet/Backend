namespace TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

using TribeWallet.Infrastructure;
public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public Usuario GetById(Guid id)
    {
        return _repository.GetById(id);
    }
    public IEnumerable<Usuario> GetAll()
    {
        return _repository.GetAll();
    }

    public Usuario Create(Usuario usuario)
    {
        /*int usuarioId = new Random().Next(1, 100);

        usuario.UsuarioId = usuarioId;
        
        return usuario;*/
        throw new NotImplementedException();
    }

    public Usuario Login(LoginDTO loginDto)
    {
        //var usuario = _repository.Login(loginDto);
        //return usuario;

        throw new NotImplementedException();
    }
}