using Microsoft.EntityFrameworkCore;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class UsuarioRepositoryImplementation : IUsuarioRepository
{
    private readonly AppDbContext _dbContext;
    public UsuarioRepositoryImplementation(AppDbContext dbContext)
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
        var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Email == loginDto.Email);
        if (usuario == null || usuario.HashSenha != loginDto.Senha)
            throw new UnauthorizedAccessException("Email ou senha incorretos");
            
        return usuario;
    }
}