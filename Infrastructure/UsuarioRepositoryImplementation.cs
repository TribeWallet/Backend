using Microsoft.EntityFrameworkCore;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class UsuarioRepositoryImplementation : IUsuarioRepository
{
    private readonly DbContext _dbContext;
    public UsuarioRepositoryImplementation(DbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Usuario GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Usuario> GetAll()
    {
        IEnumerable<Usuario> usuarios = _dbContext.Set<Usuario>();
        return usuarios;
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

        //return GetById(1);
        throw new NotImplementedException();
    }
}