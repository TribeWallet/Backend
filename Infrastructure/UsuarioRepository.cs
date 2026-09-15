using Microsoft.EntityFrameworkCore;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Data;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _dbContext;
    public UsuarioRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    //TODO criar coluna calculada "Nome Completo", essa query não está funcionando como deveria
    public async Task<IEnumerable<Usuario>> GetByNome(string nome)
    {
        var usuarios = await _dbContext.Usuarios
            .Where(u => u.Nome.ToLower().Contains(nome.ToLower()) 
                        || u.Sobrenome.ToLower().Contains(nome.ToLower()))
            .ToListAsync();

        return usuarios;
    }

    public async Task<IEnumerable<Usuario>> GetAll(bool deleted)
    {
        List<Usuario> usuarios;
        
        // !deleted significa que ele vai buscar apenas registros ativos (deletedAt == null)
        if (!deleted)
        {
            usuarios = await _dbContext.Usuarios
                .Where(u => u.DeletedAt == null).ToListAsync();
            
            return usuarios;
        }
        
        // busca registros ativos e inativos
        usuarios = await _dbContext.Usuarios.ToListAsync();
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

    /// <summary>
    /// Soft delete: a linha continua no banco, só passa a carregar a data da exclusão. Repetir a
    /// chamada não mexe na data original, então o horário guardado é sempre o da primeira exclusão.
    /// </summary>
    public async Task<Usuario> Delete(Usuario usuario)
    {
        if (usuario.DeletedAt is null)
        {
            usuario.DeletedAt = DateTime.UtcNow;
            _dbContext.Usuarios.Update(usuario);
            await _dbContext.SaveChangesAsync();
        }

        return usuario;
    }

    public async Task<Usuario> Login(LoginRequestDTO loginRequestDto)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == loginRequestDto.Email);
        if (usuario == null)
            throw new UnauthorizedAccessException("Usuário ou senha incorretos");
        
        var isValid = BCrypt.Net.BCrypt.Verify(loginRequestDto.Senha, usuario.HashSenha);
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