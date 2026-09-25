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
    
    /*public async Task<IEnumerable<UsuarioResponseDTO>> GetAll(bool deleted = false)
    {
        var usuarios = await _repository.GetAll(deleted);
        return usuarios.Select(ConvertToDto).ToList();
    }*/ //codigo antigo. esse bloco abaixo substitui para compilar
    // O serviço passa a aceitar o parâmetro e repassa para o repositório
    public async Task<IEnumerable<UsuarioResponseDTO>> GetAll(bool deleted = false)
    {
        var usuarios = await _repository.GetAll(deleted); 
        
        // Supondo que você use LINQ ou um laço para converter, a estrutura é parecida com esta:
        return usuarios.Select(u => new UsuarioResponseDTO
        {
            UsuarioToken = u.Token,
            Nome = u.Nome,
            Sobrenome = u.Sobrenome,
            Email = u.Email,
            Username = u.Username,
            DeletedAt = u.DeletedAt
        }).ToList();
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
        return ConvertToDto(usuario);
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
    {
        var usuario = await _repository.Login(loginRequestDto);
        var jwtToken = _jwtTokenService.GenerateToken(usuario);

        var loginResponseDto = new LoginResponseDTO
        {
            UsuarioResponseDto = ConvertToDto(usuario),
            JwtToken = jwtToken,
        };
        return loginResponseDto;
    }

    public async Task<UsuarioResponseDTO> Update(UpdateUsuarioRequestDTO updateUsuarioRequestDto, string usuarioToken)
    {
        var usuario = await GetByToken(usuarioToken);
        usuario.Nome = updateUsuarioRequestDto.Nome ?? usuario.Nome;
        usuario.Sobrenome = updateUsuarioRequestDto.Sobrenome ?? usuario.Sobrenome;
        usuario.Username = updateUsuarioRequestDto.Username ?? usuario.Username;
        usuario.Imagem = updateUsuarioRequestDto.Imagem ?? usuario.Imagem;
        usuario.HashSenha = updateUsuarioRequestDto.Senha == null ?  usuario.HashSenha : HashSenha(updateUsuarioRequestDto.Senha);
        
        var newUsuario = await _repository.Update(usuario);
        return ConvertToDto(newUsuario);
    }

    /// <summary>Marca o usuário como excluído. O registro fica no banco com a data em DeletedAt.</summary>
    public async Task Delete(string usuarioToken)
    {
        var usuario = await GetByToken(usuarioToken);
        await _repository.Delete(usuario);
    }

    public async Task<Usuario> GetByToken(string token)
    {
        var usuario =  await _repository.GetByToken(token);
        return usuario;
    }

    public async Task<List<UsuarioResponseDTO>> GetByNome(string nome)
    {
        var usuarios = await  _repository.GetByNome(nome);
        var responseDtoList = new List<UsuarioResponseDTO>();

        foreach (var usuario in usuarios)
        {
            var responseDto = ConvertUsuarioToResponseDto(usuario);
            responseDtoList.Add(responseDto);
        }

        return responseDtoList;
    }
    private static UsuarioResponseDTO ConvertToDto(Usuario usuario)
    {
        return new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username,
            DeletedAt = usuario.DeletedAt
        };
    }

    private static string HashSenha(string senha)
    {
        return BCrypt.Net.BCrypt.HashPassword(senha, FatorBCrypt);
    }

    private UsuarioResponseDTO ConvertUsuarioToResponseDto(Usuario usuario)
    {
        var reponseDto = new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username
        };

        return reponseDto;
    }
}