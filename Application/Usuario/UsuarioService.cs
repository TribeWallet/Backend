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

    public async Task<UsuarioResponseDTO> Create(CreateUsuarioRequestDTO createUsuarioRequestDto)
    {
        var usuario = new Usuario
        {
            Nome = createUsuarioRequestDto.Nome,
            Sobrenome = createUsuarioRequestDto.Sobrenome,
            Email = createUsuarioRequestDto.Email,
            Username = createUsuarioRequestDto.Username,
            Imagem = createUsuarioRequestDto.Imagem ?? "",
            HashSenha = HashSenha(createUsuarioRequestDto.Senha)
        };
        usuario = await _repository.Create(usuario);

        var usuarioResponseDto = new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username,
        };
        return usuarioResponseDto;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
    {
        var usuario = await _repository.Login(loginRequestDto);
        var jwtToken = _jwtTokenService.GenerateToken(usuario);
        var responseDto = new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username,
        };

        var loginResponseDto = new LoginResponseDTO
        {
            UsuarioResponseDto = responseDto,
            JwtToken = jwtToken,
        };
        return loginResponseDto;
    }

    public async Task<UsuarioResponseDTO> Update(EditUsuarioDTO editUsuarioDto, string usuarioToken)
    {
        var usuario = await GetByToken(usuarioToken);
        usuario.Nome = editUsuarioDto.Nome ?? usuario.Nome;
        usuario.Sobrenome = editUsuarioDto.Sobrenome ?? usuario.Sobrenome;
        usuario.Username = editUsuarioDto.Username ?? usuario.Username;
        usuario.Imagem = editUsuarioDto.Imagem ?? usuario.Imagem;
        usuario.HashSenha = editUsuarioDto.Senha == null ?  usuario.HashSenha : HashSenha(editUsuarioDto.Senha);
        
        var newUsuario = await _repository.Update(usuario);
        var responseDto = new UsuarioResponseDTO
        {
            UsuarioToken = newUsuario.Token,
            Nome = newUsuario.Nome,
            Sobrenome = newUsuario.Sobrenome,
            Email = newUsuario.Email,
            Username = newUsuario.Username
        };
        
        return responseDto;
    }

    public async Task<Usuario> GetByToken(string token)
    {
        var usuario =  await _repository.GetByToken(token);
        return usuario;
    }
    private static string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, FatorBCrypt);
    }
    
}