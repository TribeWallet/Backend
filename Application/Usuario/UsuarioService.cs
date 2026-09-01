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
    
    public async Task<IEnumerable<Usuario>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<UsuarioReturnDTO> Create(UsuarioRegisterDTO usuarioRegisterDto)
    {
        var usuario = new Usuario
        {
            Nome = usuarioRegisterDto.Nome,
            Sobrenome = usuarioRegisterDto.Sobrenome,
            Email = usuarioRegisterDto.Email,
            Username = usuarioRegisterDto.Username,
            Imagem = usuarioRegisterDto.Imagem ?? "",
            HashSenha = HashSenha(usuarioRegisterDto.Senha)
        };
        
        usuario = await _repository.Create(usuario);

        var returnUsuarioDto = new UsuarioReturnDTO
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

    public async Task<UsuarioReturnDTO> Login(LoginDTO loginDto)
    {
        var usuario = await _repository.Login(loginDto);
        var jwtToken = _jwtTokenService.GenerateToken(usuario);
        var returnDto = new UsuarioReturnDTO
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

    public async Task<UsuarioReturnDTO> Update(EditUsuarioDTO editUsuarioDto, string usuarioToken)
    {
        var usuario = await GetByToken(usuarioToken);
        usuario.Nome = editUsuarioDto.Nome ?? usuario.Nome;
        usuario.Sobrenome = editUsuarioDto.Sobrenome ?? usuario.Sobrenome;
        usuario.Username = editUsuarioDto.Username ?? usuario.Username;
        usuario.Imagem = editUsuarioDto.Imagem ?? usuario.Imagem;
        usuario.HashSenha = editUsuarioDto.Senha == null ?  usuario.HashSenha : HashSenha(editUsuarioDto.Senha);
        
        var newUsuario = await _repository.Update(usuario);
        var returnDto = new UsuarioReturnDTO
        {
            UsuarioToken = newUsuario.Token,
            Nome = newUsuario.Nome,
            Sobrenome = newUsuario.Sobrenome,
            Email = newUsuario.Email,
            Username = newUsuario.Username
        };
        
        return returnDto;
    }

    private async Task<Usuario> GetByToken(string token)
    {
        var usuario =  await _repository.GetByToken(token);
        return usuario;
    }
    private static string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, FatorBCrypt);
    }
    
}