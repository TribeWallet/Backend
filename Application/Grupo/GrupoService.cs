using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Infrastructure;
using TribeWallet.Services;

namespace TribeWallet.Application.Grupo;
using TribeWallet.Domain.Entities;
using TribeWallet.Application.Grupo.DTOs;

public class GrupoService
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IIntegranteRepository _integranteRepository;
    private readonly IIntegranteCompromissoRepository  _integranteCompromissoRepository;
    private readonly IntegranteService _integranteService;
    private readonly GrupoIntegranteCommonService _grupoIntegranteCommonService;
    private readonly CompromissoFinanceiroService _compromissoFinanceiroService;
    
    public GrupoService(IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IntegranteService integranteService, GrupoIntegranteCommonService grupoIntegranteCommonService, CompromissoFinanceiroService compromissoFinanceiroService, IIntegranteCompromissoRepository integranteCompromissoRepository)
    {
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteService = integranteService;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
        _compromissoFinanceiroService = compromissoFinanceiroService;
        _integranteCompromissoRepository = integranteCompromissoRepository;
    }

    public async Task<List<GrupoResponseDTO>> GetAllByUsuarioToken(string token, bool deleted)
    {
        var grupos = await _grupoRepository.GetAllByUsuarioToken(token, deleted);
        var responseDto = new List<GrupoResponseDTO>();

        foreach (var grupo in grupos)
        {
            var compromissosResponseDtoList = new List<CompromissoFinanceiroResponseDTO>();
            foreach (var compromisso in grupo.Compromissos)
            {
                
                //em ConvertCompromissoToResponseDto, parcial não busca o grupo de novo e adiciona dentro de compromissos.
                var compromissoResponseDto = await _compromissoFinanceiroService.ConvertCompromissoToResponseDto(compromisso, parcial: true);
                compromissosResponseDtoList.Add(compromissoResponseDto);
            }
            var integrantesDto = await _integranteService.GetAllByGrupoToken(grupo.Token, deleted);
            var grupoDto = new GrupoResponseDTO
            {
                GrupoToken =  grupo.Token,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Integrantes = integrantesDto,
                Compromissos = compromissosResponseDtoList
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

    /// <summary>Marca o grupo como excluído. O registro fica no banco com a data em DeletedAt.</summary>
    public async Task Delete(string grupoToken)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);
        await _grupoRepository.Delete(grupo);
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
                var integranteResponseDto = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(newIntegrante);
                
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
     
        var responseDto = await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(grupo, deleted: false);
        return responseDto;
    }

    public async Task<GrupoResponseDTO> RemoveIntegrante(string grupoToken, string integranteToken)
    {
        var integrante = await _integranteRepository.GetByToken(integranteToken);
        var grupo = integrante.Grupo;
        var integranteCompromisso = await _integranteCompromissoRepository.GetByIntegranteToken(integranteToken);
        
        await _integranteCompromissoRepository.Delete(integranteCompromisso);
        await _integranteRepository.Delete(integrante);
        
        var responseDto = await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(grupo, deleted: false);
        return responseDto;
    }
}