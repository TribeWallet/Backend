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

    public async Task<IEnumerable<Usuario>> GetAll()
    {
        var usuarios = await _dbContext.Usuarios.ToListAsync();
        return usuarios;
    }

    public async Task<Usuario> Create(Usuario usuario)
    {
        var newUsuario = _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();
        return newUsuario.Entity;
    }

    public async Task<Usuario> Update(Usuario usuario)
    {
        var newUsuario = _dbContext.Usuarios.Update(usuario);
        await _dbContext.SaveChangesAsync();
     
        return  newUsuario.Entity;
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Usuario> Login(LoginDTO loginDto)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (usuario == null)
            throw new UnauthorizedAccessException("Usuário ou senha incorretos");
        
        var isValid = BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.HashSenha);
        if(!isValid)
            throw new UnauthorizedAccessException("Usuário ou senha incorretos");
            
        return usuario;
    }

    public async Task<Usuario> GetByToken(string token)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Token == token);
        if (usuario == null)
            throw new Exception("Usuário não encontrado pelo token informado");
        return usuario;
    }
}