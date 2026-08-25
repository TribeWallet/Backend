using TribeWallet.Domain;

namespace TribeWallet.Application;

public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public Usuario GetById(int id)
    {
        return _repository.GetById(id);
    }
    public IEnumerable<Usuario> GetAll()
    {
        return _repository.GetAll();
    }

    public Usuario Create(Usuario usuario)
    {
        int usuarioId = new Random().Next(1, 100);

        usuario.UsuarioId = usuarioId;
        
        return usuario;
    }
}