using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Integrante;
using TribeWallet.Application.IntegranteCompromisso;
using TribeWallet.Application.IntegranteCompromisso.DTOs;
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

    public CompromissoFinanceiroService(ICompromissoFinanceiroRepository compromissoFinanceiroRepository, GrupoIntegranteCommonService grupoIntegranteCommonService, IntegranteService integranteService, IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IIntegranteCompromissoRepository integranteCompromissoRepository)
    {
        _compromissoFinanceiroRepository = compromissoFinanceiroRepository;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
        _integranteService = integranteService;
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteCompromissoRepository = integranteCompromissoRepository;
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

        if (requestDto.Participacoes is not null)
        {
            foreach (var participacao in requestDto.Participacoes)
            {
                var integrante = await _integranteRepository.GetByToken(participacao.IntegranteToken);

                if (integrante is null)
                    throw new Exception("Integrante não foi encontrado pelo token informado.");
                
                var integranteCompromisso = new IntegranteCompromisso
                {
                    IntegranteId = integrante.IntegranteId,
                    CompromissoId = compromisso.CompromissoFinanceiroId,
                    ValorDevedor = participacao.ValorDevedor,
                    ValorPago = participacao.ValorPago,
                    Integrante = integrante,
                    Compromisso = compromisso
                };

                await _integranteCompromissoRepository.Create(integranteCompromisso);
            }
        }
        
        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);
        
        return responseDto;
    }

    public async Task<CompromissoFinanceiroResponseDTO> UpdateCompromissoFinanceiro(
        UpdateCompromissoFinanceiroRequestDTO requestDto, string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);
        compromisso.Titulo = requestDto.Titulo ?? compromisso.Titulo;
        compromisso.ValorTotal = requestDto.ValorTotal ?? compromisso.ValorTotal;
        compromisso.Data = requestDto.Data ?? compromisso.Data;
        compromisso.TipoDivisao = requestDto.TipoDivisao ?? compromisso.TipoDivisao;
        compromisso.Imagem = requestDto.Imagem ?? compromisso.Imagem;
        compromisso.Categoria = requestDto.Categoria ?? compromisso.Categoria;
        
        await _compromissoFinanceiroRepository.Update(compromisso);

        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);
        
        return  responseDto;
    }
    public async Task DeleteCompromissoFinanceiro(string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);

        await _compromissoFinanceiroRepository.Delete(compromisso);
    }
    public async Task<CompromissoFinanceiroResponseDTO> AddIntegrante(List<CreateIntegranteCompromissoRequestDTO> requestDtoList, string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);

        foreach (var requestDto in requestDtoList)
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
        compromisso = await _compromissoFinanceiroRepository.Update(compromisso);
        
        var responseDto = await ConvertCompromissoToResponseDto(compromisso, parcial: false);

        return responseDto;
    }

    public async Task RemoveIntegrante(string integranteCompromissoToken)
    {
        var integranteCompromisso = await _integranteCompromissoRepository.GetByToken(integranteCompromissoToken);
        
        await _integranteCompromissoRepository.Delete(integranteCompromisso);
    }
 
    #region CONVERSÕES
    public IntegranteCompromissoResumoDTO ConvertIntegranteCompromissoToResumoDto(
        Domain.Entities.IntegranteCompromisso integranteCompromisso)
    {
        var responseDto = new IntegranteCompromissoResumoDTO()
        {
            IntegranteCompromissoToken = integranteCompromisso.Token,
            Integrante = _grupoIntegranteCommonService.ConvertIntegranteToResponseDto(integranteCompromisso.Integrante),
            ValorDevedor = integranteCompromisso.ValorDevedor,
            ValorPago = integranteCompromisso.ValorPago,
            DeletedAt = integranteCompromisso.DeletedAt
            //TODO PAGAMENTOS
        };

        return responseDto;
    }


    public async Task<CompromissoFinanceiroResponseDTO> ConvertCompromissoToResponseDto(
        Domain.Entities.CompromissoFinanceiro compromisso, bool parcial)
    {
        var participacoes = new List<IntegranteCompromissoResumoDTO>();
        foreach (var participacao in compromisso.Participacoes)
        {
            var participacaoDto = ConvertIntegranteCompromissoToResumoDto(participacao);
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
            DeletedAt = compromisso.DeletedAt
            //TODO RELATORIOS
        };

        if (!parcial)
            responseDto.Grupo =
                await _grupoIntegranteCommonService.ConvertGrupoToResponseDto(compromisso.Grupo, deleted: false);


        return responseDto;
    }
    #endregion
}