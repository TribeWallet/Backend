namespace TribeWallet.Application.Usuario;
using TribeWallet.Services;
using TribeWallet.Domain.Entities;

public class UsuarioService
{
    /// <summary>Custo do BCrypt. Cada incremento dobra o tempo de verificação.</summary>
    private const int FatorBCrypt = 12;
    
    private readonly IUsuarioRepository _repository;
    private readonly TokenService _tokenService;
    
    public UsuarioService(IUsuarioRepository repository, TokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
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

    public ReturnUsuarioDTO Login(LoginDTO loginDto)
    {
        var usuario = _repository.Login(loginDto);
        var jwtToken = _tokenService.GenerateToken(usuario);
        var returnDto = new ReturnUsuarioDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username,
            JwtToken = jwtToken
        };
        
        return returnDto;
    }

    private static string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, FatorBCrypt);
    }
}