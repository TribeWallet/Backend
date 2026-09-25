using System.Diagnostics.CodeAnalysis;
using TribeWallet.Application;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain.Entities;
using TribeWallet.Services;

namespace TribeWallet.Infrastructure;

public class IntegranteService
{
    private readonly IIntegranteRepository _integranteRepository;
    private readonly UsuarioService _usuarioService;
    private readonly IGrupoRepository _grupoRepository;
    private readonly GrupoIntegranteCommonService _grupoIntegranteCommonService;

    public IntegranteService(IIntegranteRepository integranteRepository, UsuarioService usuarioService, IGrupoRepository grupoRepository, GrupoIntegranteCommonService grupoIntegranteCommonService)
    {
        _integranteRepository = integranteRepository;
        _usuarioService = usuarioService;
        _grupoRepository = grupoRepository;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
    }

    public async Task<IntegranteResponseDTO> GetByToken(string integranteToken)
    {
        var integrante = await _integranteRepository.GetByToken(integranteToken);
        var responseDto = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(integrante);
        
        return responseDto;
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
            var responseDto = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(newIntegrante);
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
            var integranteDto = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(integrante);
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

}