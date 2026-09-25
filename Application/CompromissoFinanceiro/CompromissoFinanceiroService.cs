using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Application.IntegranteCompromisso.DTOs;
using TribeWallet.Application.Pagamento;
using TribeWallet.Infrastructure;
using TribeWallet.Services;

namespace TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Domain.Entities;

public class CompromissoFinanceiroService
{
    private readonly ICompromissoFinanceiroRepository _compromissoFinanceiroRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly IIntegranteRepository  _integranteRepository;
    private readonly IIntegranteCompromissoRepository  _integranteCompromissoRepository;
    private readonly GrupoIntegranteCommonService _grupoIntegranteCommonService;
    private readonly IntegranteService _integranteService;
    private readonly PagamentoService _pagamentoService;

    public CompromissoFinanceiroService(ICompromissoFinanceiroRepository compromissoFinanceiroRepository, GrupoIntegranteCommonService grupoIntegranteCommonService, IntegranteService integranteService, IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IIntegranteCompromissoRepository integranteCompromissoRepository, PagamentoService pagamentoService)
    {
        _compromissoFinanceiroRepository = compromissoFinanceiroRepository;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
        _integranteService = integranteService;
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteCompromissoRepository = integranteCompromissoRepository;
        _pagamentoService = pagamentoService;
    }

    public async Task<List<CompromissoFinanceiroResponseDTO>> GetAllByIntegranteToken(string integranteToken)
    {
        var compromissos = await _compromissoFinanceiroRepository.GetAllByIntegranteToken(integranteToken);
        
        var responseDto = new List<CompromissoFinanceiroResponseDTO>();

        foreach (var compromisso in compromissos)
        {
            var compromissoResponseDto =  await ConvertCompromissoToResponseDto(compromisso, parcial: false);
            responseDto.Add(compromissoResponseDto);
        }
        return responseDto;
    }

    public async Task<List<CompromissoFinanceiroResponseDTO>> GetAllByGrupoToken(string grupoToken, bool deleted)
    {
        var compromissos = await _compromissoFinanceiroRepository.GetAllByGrupoToken(grupoToken, deleted);

        var responseDto = new List<CompromissoFinanceiroResponseDTO>();

        foreach (var compromisso in compromissos)
        {
            var compromissoResponseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);
            responseDto.Add(compromissoResponseDto);
        }

        return responseDto;
    }

    public async Task<CompromissoFinanceiroResponseDTO> CreateCompromissoFinanceiro(
        CreateCompromissoFinanceiroRequestDTO requestDto, string grupoToken)
    {
        var grupo = await _grupoRepository.GetByToken(grupoToken);
        ICollection<IntegranteCompromisso> participacoes = new List<IntegranteCompromisso>();
        var compromisso = new CompromissoFinanceiro
        {
            GrupoId = grupo.GrupoId,
            Titulo = requestDto.Titulo,
            ValorTotal = requestDto.ValorTotal,
            Data = requestDto.Data,
            TipoDivisao = requestDto.TipoDivisao,
            Imagem = requestDto.Imagem,
            Categoria = requestDto.Categoria,
            Grupo = grupo
        };
        await _compromissoFinanceiroRepository.Create(compromisso);

        if (requestDto.Participacoes.Count > 0)
        {
            participacoes = await SetUpParticipacoesEntities(requestDto.Participacoes, compromisso);
            var divisao = new DividirValorRecord(compromisso, participacoes, requestDto.ValorTotal, requestDto.TipoDivisao);
            participacoes = AssignValorDevedorAndSave(divisao);
            await _integranteCompromissoRepository.CreateMultiple(participacoes);
        }
        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);
        return responseDto;
    }
    
    
    public async Task<CompromissoFinanceiroResponseDTO> UpdateCompromissoFinanceiro(
        UpdateCompromissoFinanceiroRequestDTO requestDto, string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);
        var participacoes = await _integranteCompromissoRepository.GetAllByCompromissoToken(compromissoToken);
        
        compromisso.Titulo = requestDto.Titulo ?? compromisso.Titulo;
        compromisso.ValorTotal = requestDto.ValorTotal ?? compromisso.ValorTotal;
        compromisso.Data = requestDto.Data ?? compromisso.Data;
        compromisso.TipoDivisao = requestDto.TipoDivisao ?? compromisso.TipoDivisao;
        compromisso.Imagem = requestDto.Imagem ?? compromisso.Imagem;
        compromisso.Categoria = requestDto.Categoria ?? compromisso.Categoria;

        if (participacoes.Count > 0)
        {
            participacoes = IncludeValorExatoInParticipacao(requestDto.Participacoes, participacoes);
            var divisao = new DividirValorRecord(compromisso, participacoes, compromisso.ValorTotal, compromisso.TipoDivisao);
            participacoes = AssignValorDevedorAndSave(divisao);
            foreach (var participacao in participacoes)
            {   
                await _integranteCompromissoRepository.Update(participacao);
            }
        }
        await _compromissoFinanceiroRepository.Update(compromisso);

        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);
        
        return  responseDto;
    }
    public async Task DeleteCompromissoFinanceiro(string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);
        foreach (var participacao in compromisso.Participacoes)
        {
            var integranteCompromisso = await _integranteCompromissoRepository.GetByIntegranteToken(participacao.Token);
            await _integranteCompromissoRepository.Delete(integranteCompromisso);
        }
        await _compromissoFinanceiroRepository.Delete(compromisso);
    }
    public async Task<CompromissoFinanceiroResponseDTO> AddIntegrante(List<CreateIntegranteCompromissoRequestDTO> requestDtoList, string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);

        foreach (var requestDto in requestDtoList)
        {
            var existingParticipacao = await _integranteCompromissoRepository.GetByIntegranteToken(requestDto.IntegranteToken);
            if (existingParticipacao != null)
            {
                var integrante = await _integranteRepository.GetByToken(requestDto.IntegranteToken);
                var integranteCompromisso = new IntegranteCompromisso
                {
                    IntegranteId = integrante.IntegranteId,
                    CompromissoId = compromisso.CompromissoFinanceiroId,
                    ValorDevedor = requestDto.ValorDevedor,
                    ValorPago = requestDto.ValorPago,
                    Integrante = integrante,
                    Compromisso = compromisso
                };
                compromisso.Participacoes.Add(integranteCompromisso);
            }
        }

        compromisso = await _compromissoFinanceiroRepository.Update(compromisso);
        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);

        return responseDto;
    }

    public async Task RemoveIntegrante(string integranteCompromissoToken)
    {
        var integranteCompromisso = await _integranteCompromissoRepository.GetByToken(integranteCompromissoToken);
        
        await _integranteCompromissoRepository.Delete(integranteCompromisso);
    }
    
    //TODO refatorar funções de calculos, no momento só aceita DTOs de Create
    #region CALCULOS DE VALORES
    
    private ICollection<IntegranteCompromisso> AssignValorDevedorAndSave(DividirValorRecord divisao)
    {
        var (compromisso, participacoes, valorTotal, tipoDivisao) = divisao;
        foreach (var participacao in participacoes)
        {
            switch (tipoDivisao)
            {
                case TipoDivisao.Igual:
                    participacao.ValorDevedor = AssignValorIgual(compromisso, participacoes);
                    break;
                case TipoDivisao.Porcentagem:
                    participacao.ValorDevedor = AssignValorIgual(compromisso, participacoes);
                    break;
                case TipoDivisao.ValorExato:
                    participacao.ValorDevedor = AssignValorExato(participacao);
                    break;
                case TipoDivisao.Proporcional:
                    participacao.ValorDevedor = AssignValorIgual(compromisso, participacoes);
                    break;
                default:
                    throw new Exception("selecione um tipo de divisão válido (1 a 4).");
            }
        }

        return participacoes;
    }

    private decimal AssignValorExato(IntegranteCompromisso participacao)
    {
        return participacao.ValorDevedor;
    }

    private decimal AssignValorIgual(CompromissoFinanceiro compromisso, ICollection<IntegranteCompromisso> participacoes)
    {
        return compromisso.ValorTotal / participacoes.Count;
    }
    
    #endregion
    
    #region CONVERSÕES

    private async Task<List<IntegranteCompromisso>> SetUpParticipacoesEntities(ICollection<CreateIntegranteCompromissoRequestDTO> participacoesDto, CompromissoFinanceiro compromisso)
    {
        var participacoes = new List<IntegranteCompromisso>();
        foreach (var participacao in participacoesDto)
        {
            var integrante = await _integranteRepository.GetByToken(participacao.IntegranteToken);

            if (integrante is null)
                throw new Exception("Integrante não foi encontrado pelo token informado.");

            var integranteCompromisso = new IntegranteCompromisso
            {
                IntegranteId = integrante.IntegranteId,
                CompromissoId = compromisso.CompromissoFinanceiroId,
                ValorPago = participacao.ValorPago,
                ValorDevedor = participacao.ValorDevedor,
                Integrante = integrante,
                Compromisso = compromisso
            };
            participacoes.Add(integranteCompromisso);
        }

        return participacoes;
    }

    private ICollection<IntegranteCompromisso> IncludeValorExatoInParticipacao(ICollection<CreateIntegranteCompromissoRequestDTO> requestDtoList,
        ICollection<IntegranteCompromisso> participacoes)
    {
        foreach (var requestDto in requestDtoList)
        {
            foreach (var participacao in participacoes)
            {
                if (participacao.Integrante.Token == requestDto.IntegranteToken)
                {
                    participacao.ValorDevedor = requestDto.ValorDevedor;
                }
            }
        }

        return participacoes;
    }
    public async Task<IntegranteCompromissoResumoDTO> ConvertIntegranteCompromissoToResumoDto(
        IntegranteCompromisso integranteCompromisso)
    {
        
        var pagamentos = await _pagamentoService.GetPagamentosByIntegranteCompromissoToken(integranteCompromisso.Token);
        var responseDto = new IntegranteCompromissoResumoDTO()
        {
            IntegranteCompromissoToken = integranteCompromisso.Token,
            Integrante = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(integranteCompromisso.Integrante),
            ValorDevedor = integranteCompromisso.ValorDevedor,
            ValorPago = integranteCompromisso.ValorPago,
            DeletedAt = integranteCompromisso.DeletedAt,
            Pagamentos = pagamentos 
        };

        return responseDto;
    }


    public async Task<CompromissoFinanceiroResponseDTO> ConvertCompromissoToResponseDto(
        CompromissoFinanceiro compromisso, bool parcial)
    {
        var participacoes = new List<IntegranteCompromissoResumoDTO>();
        foreach (var participacao in compromisso.Participacoes)
        {
            var participacaoDto = await ConvertIntegranteCompromissoToResumoDto(participacao);
            participacoes.Add(participacaoDto);
        }

        
        var responseDto = new CompromissoFinanceiroResponseDTO
        {
            CompromissoFinanceiroToken = compromisso.Token,
            Titulo = compromisso.Titulo,
            ValorTotal = compromisso.ValorTotal,
            Data = compromisso.Data,
            TipoDivisao = compromisso.TipoDivisao,
            Imagem = compromisso.Imagem,
            Categoria = compromisso.Categoria,
            Participacoes = participacoes,
            DeletedAt = compromisso.DeletedAt,
            //TODO RELATORIOS
        };

        if (!parcial)
            responseDto.Grupo =
                await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(compromisso.Grupo, deleted: false);


        return responseDto;
    }
    #endregion
    
}