using System.Diagnostics.CodeAnalysis;
using TribeWallet.Application;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Infrastructure;

public class IntegranteService
{
    private readonly IIntegranteRepository _integranteRepository;
    private readonly UsuarioService _usuarioService;
    private readonly IGrupoRepository _grupoRepository;

    public IntegranteService(IIntegranteRepository integranteRepository, UsuarioService usuarioService, IGrupoRepository grupoRepository)
    {
        _integranteRepository = integranteRepository;
        _usuarioService = usuarioService;
        _grupoRepository = grupoRepository;
    }

    public async Task<List<IntegranteResponseDTO>> AddIntegranteToGrupo(List<CreateIntegranteRequestDTO> requestDto,
        string grupoToken)
    {
        var responseDtoList = new List<IntegranteResponseDTO>();
        
        //busca o grupo a quem esses integrantes serão adicionados
        var grupo = await  _grupoRepository.GetByToken(grupoToken);
        foreach (var integrante in requestDto)
        {
            //cada integrante é um usuário no sistema
            var  usuario = await _usuarioService.GetByToken(integrante.UsuarioToken);
            
            //persiste integrante no banco
            var newIntegrante = new Integrante
            {
                UsuarioId = usuario.UsuarioId,
                GrupoId = grupo.GrupoId,
                Usuario = usuario,
                Grupo = grupo
            };

            await _integranteRepository.Create(newIntegrante);
            
            //converte integrante e usuario em responseDTOs aninhados
            var responseDto = ConvertIntegranteToResponseDto(newIntegrante, grupoToken);
            responseDtoList.Add(responseDto);
        }
        
        return responseDtoList;
    }

    public async Task<ICollection<IntegranteResponseDTO>> GetAllByGrupoToken(string grupoToken, bool deleted)
    {
        var integrantes = await _integranteRepository.GetAllByGrupoToken(grupoToken, deleted);
        var responseDto = new List<IntegranteResponseDTO>();
        foreach (var integrante in integrantes)
        {
            var integranteDto = ConvertIntegranteToResponseDto(integrante, grupoToken);
            responseDto.Add(integranteDto);
        }
        
        return responseDto;
    }

    public async Task<Integrante> SetupIntegranteEntity(CreateIntegranteRequestDTO createIntegranteRequestDto,
        string grupoToken)
    {
        var usuario = await _usuarioService.GetByToken(createIntegranteRequestDto.UsuarioToken);
        var integrante = new Integrante
        {
            UsuarioId = usuario.UsuarioId,
            Usuario = usuario
        };

        return integrante;
    }
    public IntegranteResponseDTO ConvertIntegranteToResponseDto(Integrante integrante, string grupoToken)
    {
        var usuarioDto = ConvertUsuarioToResponseDto(integrante.Usuario);
        var integranteDto = new IntegranteResponseDTO
        {
            IntegranteToken = integrante.Token,
            Usuario = usuarioDto,
            GrupoToken = grupoToken
            //TODO adicionar compromissos
        };
        
        return integranteDto;
    }

    private UsuarioResponseDTO ConvertUsuarioToResponseDto(Usuario usuario)
    {
        var usuarioDto = new UsuarioResponseDTO
        {
            UsuarioToken = usuario.Token,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Username = usuario.Username
        };
        
        return usuarioDto;
    }
}