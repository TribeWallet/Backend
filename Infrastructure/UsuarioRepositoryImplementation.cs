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

    public Usuario Create(Usuario usuario)
    {
        var newUsuario = _dbContext.Usuarios.Add(usuario);
        _dbContext.SaveChanges();
        return newUsuario.Entity;
    }

    public void Add(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    public Usuario Update(Usuario usuario)
    {
        var newUsuario = _dbContext.Usuarios.Update(usuario);
        _dbContext.SaveChanges();
     
        return  newUsuario.Entity;
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Usuario Login(LoginDTO loginDto)
    {
        //logica de login
        var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Email == loginDto.Email);
        if (usuario == null)
            throw new UnauthorizedAccessException("Usuário ou senha incorretos");
        
        var isValid = BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.HashSenha);
        if(!isValid)
            throw new UnauthorizedAccessException("Usuário ou senha incorretos");
            
        return usuario;
    }

    public Usuario GetByToken(string token)
    {
        var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Token == token);
        if (usuario == null)
            throw new Exception("Usuário não encontrado pelo token informado");
        return usuario;
    }
}