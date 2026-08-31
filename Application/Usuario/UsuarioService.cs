namespace TribeWallet.Application.Usuario;
using TribeWallet.Services;
using TribeWallet.Domain.Entities;

public class UsuarioService
{
    /// <summary>Custo do BCrypt. Cada incremento dobra o tempo de verificação.</summary>
    private const int FatorBCrypt = 12;
    
    private readonly IUsuarioRepository _repository;
    private readonly JwtTokenService _jwtTokenService;
    
    public UsuarioService(IUsuarioRepository repository, JwtTokenService jwtTokenService)
    {
        _repository = repository;
        _jwtTokenService = jwtTokenService;
    }

    public Usuario GetById(Guid id)
    {
        return _repository.GetById(id);
    }
    public IEnumerable<Usuario> GetAll()
    {
        return _repository.GetAll();
    }

    public ReturnUsuarioDTO Create(RegisterUsuarioDTO registerUsuarioDto)
    {
        var usuario = new Usuario
        {
            Nome = registerUsuarioDto.Nome,
            Sobrenome = registerUsuarioDto.Sobrenome,
            Email = registerUsuarioDto.Email,
            Username = registerUsuarioDto.Username,
            Imagem = registerUsuarioDto.Imagem ?? "",
            HashSenha = HashSenha(registerUsuarioDto.Senha)
        };
        
        usuario = _repository.Create(usuario);

        var returnUsuarioDto = new ReturnUsuarioDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username,
            JwtToken = _jwtTokenService.GenerateToken(usuario)
        };
        return returnUsuarioDto;
    }

    public ReturnUsuarioDTO Login(LoginDTO loginDto)
    {
        var usuario = _repository.Login(loginDto);
        var jwtToken = _jwtTokenService.GenerateToken(usuario);
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

    public ReturnUsuarioDTO Update(EditUsuarioDTO editUsuarioDto, string usuarioToken)
    {
        var usuario = GetByToken(usuarioToken);
        usuario.Nome = editUsuarioDto.Nome ?? usuario.Nome;
        usuario.Sobrenome = editUsuarioDto.Sobrenome ?? usuario.Sobrenome;
        usuario.Username = editUsuarioDto.Username ?? usuario.Username;
        usuario.Imagem = editUsuarioDto.Imagem ?? usuario.Imagem;
        usuario.HashSenha = editUsuarioDto.Senha == null ?  usuario.HashSenha : HashSenha(editUsuarioDto.Senha);
        
        var newUsuario = _repository.Update(usuario);
        var returnDto = new ReturnUsuarioDTO
        {
            UsuarioToken = newUsuario.Token,
            Nome = newUsuario.Nome,
            Sobrenome = newUsuario.Sobrenome,
            Email = newUsuario.Email,
            Username = newUsuario.Username
        };
        
        return returnDto;
    }

    private Usuario GetByToken(string token)
    {
        var usuario =  _repository.GetByToken(token);
        return usuario;
    }
    private static string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, FatorBCrypt);
    }
    
}