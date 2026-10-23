using TribeWallet.Application.Arquivo;
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
    private readonly ArquivoLocalService _arquivoLocalService;
    
    public GrupoService(IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IntegranteService integranteService, GrupoIntegranteCommonService grupoIntegranteCommonService, CompromissoFinanceiroService compromissoFinanceiroService, IIntegranteCompromissoRepository integranteCompromissoRepository, ArquivoLocalService arquivoLocalService)
    {
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteService = integranteService;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
        _compromissoFinanceiroService = compromissoFinanceiroService;
        _integranteCompromissoRepository = integranteCompromissoRepository;
        _arquivoLocalService = arquivoLocalService;
    }

    public async Task<List<GrupoResponseMainDTO>> GetAllByUsuarioToken(string token, bool deleted)
    {
        var grupos = await _grupoRepository.GetAllByUsuarioToken(token, deleted);
        var reponseList = new List<GrupoResponseMainDTO>();

        foreach (var grupo in grupos)
        {
            var responseDto = ConvertGrupoToResponseDto(grupo);
            reponseList.Add(responseDto);
        }
        
        return reponseList;
    }

    public async Task<GrupoResponseDTO> GetByToken(string grupoToken, bool deleted)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);

        var responseDto = await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(grupo, deleted);
        return responseDto;
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
            DeletedAt = grupo.DeletedAt,
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

        if (integrante is null)
            throw new Exception("Integrante não encontrado pelo token informado.");

        var grupo = integrante.Grupo;
        var integranteCompromisso = await _integranteCompromissoRepository.GetByIntegranteToken(integranteToken);

        // Integrante que nunca entrou num compromisso não tem participação para excluir.
        if (integranteCompromisso != null)
            await _integranteCompromissoRepository.Delete(integranteCompromisso);

        await _integranteRepository.Delete(integrante);
        
        var responseDto = await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(grupo, deleted: false);
        return responseDto;
    }

    private GrupoResponseMainDTO ConvertGrupoToResponseDto(Grupo grupo)
    {
     
        var valores = GetContagensEValores(grupo);
        var integrantes = ConvertIntegrantesToResponseDto(grupo.Integrantes);
        var compromissos = ConvertCompromissosToResumoDto(grupo.Compromissos);
        var responseDto = new GrupoResponseMainDTO
        {
            GrupoToken =  grupo.Token,
            Nome = grupo.Nome,
            Descricao = grupo.Descricao,
            IntegrantesCount = valores.integrantesCount,
            CompromissosCount = valores.compromissosCount,
            TotalEmAberto = valores.totalEmAberto,
            TotalPago = valores.totalPago,
            DeletedAt = grupo.DeletedAt,
            Integrantes = integrantes,
            Compromissos = compromissos
        };

        return responseDto;
    }
    
    private (int integrantesCount, int compromissosCount, decimal totalEmAberto, decimal totalPago) GetContagensEValores(
        Grupo grupo)
    {
        int integranteCount = 0;
        int compromissosCount = 0;
        decimal totalEmAberto = 0;
        decimal totalPago = 0;
        
        integranteCount = grupo.Integrantes.Count;
        compromissosCount = grupo.Compromissos.Count;

        foreach (var compromisso in grupo.Compromissos)
        {
            foreach (var participacao in compromisso.Participacoes)
            {
                totalPago += participacao.ValorPago;
                totalEmAberto += Math.Max(0, participacao.ValorDevedor - participacao.ValorPago);
            }
        }
        
        return (integranteCount, compromissosCount, totalEmAberto, totalPago);
    }

    private ICollection<IntegranteResumoDTO> ConvertIntegrantesToResponseDto(ICollection<Integrante> integrantes)
    {
        var responseList = new List<IntegranteResumoDTO>();
        foreach (var integrante in integrantes)
        {
            var responseDto = new IntegranteResumoDTO
            {
                IntegranteToken = integrante.Token,
                Nome = integrante.Usuario.Nome,
                Sobrenome = integrante.Usuario.Sobrenome,
                Email = integrante.Usuario.Email,
                ImagemUrl = _arquivoLocalService.ObterUrlLocalCompleta(integrante.Usuario.Imagem),
                DeletedAt = integrante.DeletedAt
            };
            
            responseList.Add(responseDto);
        }

        return responseList;
    }

    private ICollection<CompromissoFinanceiroResumoDTO> ConvertCompromissosToResumoDto(ICollection<CompromissoFinanceiro> compromissosFinanceiros)
    {
        var responseList = new List<CompromissoFinanceiroResumoDTO>();

        foreach (var compromisso in compromissosFinanceiros)
        {
            var responseDto = new CompromissoFinanceiroResumoDTO
            {
                CompromissoFinanceiroToken = compromisso.Token,
                Titulo = compromisso.Titulo,
                ImagemUrl = _arquivoLocalService.ObterUrlLocalCompleta(compromisso.Imagem),
                Categoria = compromisso.Categoria,
                ValorTotal = compromisso.ValorTotal,
                Data = compromisso.Data,
                DeletedAt = compromisso.DeletedAt
            };
            responseList.Add(responseDto);
        }
        return responseList;
    }
}