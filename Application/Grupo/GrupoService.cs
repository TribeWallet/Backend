using TribeWallet.Application.Integrante;
using TribeWallet.Infrastructure;

namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;
using TribeWallet.Application.Grupo.DTOs;

public class GrupoService
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IIntegranteRepository _integranteRepository;
    private readonly IntegranteService _integranteService;
    
    public GrupoService(IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IntegranteService integranteService)
    {
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteService = integranteService;
    }

    public async Task<List<GrupoResponseDTO>> GetAllByUsuarioToken(string token)
    {
        var grupos = await _grupoRepository.GetAllByUsuarioToken(token);
        var responseDto = new List<GrupoResponseDTO>();
        foreach (var grupo in grupos)
        {
            var integrantesDto = await _integranteService.GetAllByGrupoToken(grupo.Token);
            var grupoDto = new GrupoResponseDTO
            {
                GrupoToken =  grupo.Token,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Integrantes = integrantesDto
            };
            responseDto.Add(grupoDto);
        }
        
        return responseDto;
    }

    public async Task<Grupo> GetByToken(string grupoToken)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);
        return grupo;
    }

    public async Task<GrupoResponseDTO?> Create(CreateGrupoRequestDTO createGrupoRequestDto)
    {
        var grupo = new Grupo
        {
            Nome = createGrupoRequestDto.Nome,
            Descricao = createGrupoRequestDto.Descricao
        };
        grupo = await _grupoRepository.Create(grupo);
        
        var integranteResponseDtoList = new List<IntegranteResponseDTO>();
        if (createGrupoRequestDto.Integrantes.Count > 0)
        {
            foreach (var integranteRequestDto in createGrupoRequestDto.Integrantes)
            {
                //prepara entidade de integrante
                var newIntegrante = await _integranteService.SetupIntegranteEntity(integranteRequestDto,  grupo.Token);
                
                //adiciona dados do grpo na entidade de integrante
                newIntegrante.Grupo = grupo;
                newIntegrante.GrupoId = grupo.GrupoId;
                
                //persiste integrante no banco
                newIntegrante = await _integranteRepository.Create(newIntegrante);
                var integranteResponseDto = _integranteService.ConvertIntegranteToDto(newIntegrante, grupo.Token);
                
                integranteResponseDtoList.Add(integranteResponseDto);
            }
        }
        
        //cria dto de resposta dos grupos
        var responseDto = new GrupoResponseDTO
        {
            GrupoToken = grupo.Token,
            Nome = grupo.Nome,
            Descricao = grupo.Descricao,
            Integrantes =  integranteResponseDtoList
            //TODO adicionar compromissos
        };
        return responseDto;
    }

    public async Task<GrupoResponseDTO?> Update(UpdateGrupoRequestDTO updateGrupoRequestDto, string grupoToken)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);

        //verifica se Nome e Descrição existem, se existirem, atualiza
        grupo.Nome = updateGrupoRequestDto.Nome ?? grupo.Nome;
        grupo.Descricao = updateGrupoRequestDto.Descricao ?? grupo.Descricao;
        
        await _grupoRepository.Update(grupo);
        var integrantes = await _integranteService.GetAllByGrupoToken(grupoToken);

        var grupoResponseDto = new GrupoResponseDTO
        {
            GrupoToken = grupo.Token,
            Nome = grupo.Nome,
            Descricao = grupo.Descricao,
            Integrantes = integrantes
        };
        
        return grupoResponseDto;
    }
}