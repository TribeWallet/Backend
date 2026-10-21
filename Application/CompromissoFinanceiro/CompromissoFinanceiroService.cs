using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Arquivo;
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
    private readonly ArquivoLocalService _arquivoLocalService;

    public CompromissoFinanceiroService(ICompromissoFinanceiroRepository compromissoFinanceiroRepository, GrupoIntegranteCommonService grupoIntegranteCommonService, IntegranteService integranteService, IGrupoRepository grupoRepository, IIntegranteRepository integranteRepository, IIntegranteCompromissoRepository integranteCompromissoRepository, PagamentoService pagamentoService, ArquivoLocalService arquivoLocalService)
    {
        _compromissoFinanceiroRepository = compromissoFinanceiroRepository;
        _grupoIntegranteCommonService = grupoIntegranteCommonService;
        _integranteService = integranteService;
        _grupoRepository = grupoRepository;
        _integranteRepository = integranteRepository;
        _integranteCompromissoRepository = integranteCompromissoRepository;
        _pagamentoService = pagamentoService;
        _arquivoLocalService = arquivoLocalService;
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

    public async Task<List<CompromissoFinanceiroResponseMainDTO>> GetAllByUsuarioToken(string usuatioToken, bool deleted)
    {
        var compromissos = await _compromissoFinanceiroRepository.GetAllByUsuarioToken(usuatioToken, deleted);
        var responseDto = new List<CompromissoFinanceiroResponseMainDTO>();
        foreach (var compromisso in compromissos)
        {
            var compromissoResponseDto = ConvertCompromissoToResponseMainDto(compromisso);
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
        var imagem = await _arquivoLocalService.SalvarArquivoLocalAsync(requestDto.Imagem);
        var compromisso = new CompromissoFinanceiro
        {
            GrupoId = grupo.GrupoId,
            Titulo = requestDto.Titulo,
            ValorTotal = requestDto.ValorTotal,
            Data = requestDto.Data,
            TipoDivisao = requestDto.TipoDivisao,
            Imagem = imagem?.Path,
            Categoria = requestDto.Categoria,
            Grupo = grupo
        };
        await _compromissoFinanceiroRepository.Create(compromisso);

        /*if (requestDto?.Participacoes?.Count > 0)
        {
            ICollection<IntegranteCompromisso> participacoes = await SetUpParticipacoesEntities(requestDto.Participacoes, compromisso);
            var divisao = new DividirValorRecord(compromisso, participacoes, requestDto.ValorTotal, requestDto.TipoDivisao);
            participacoes = AssignValorDevedorAndSave(divisao);
            await _integranteCompromissoRepository.CreateMultiple(participacoes);
        }*/
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
        compromisso.Categoria = requestDto.Categoria ?? compromisso.Categoria;

        //caso a imagem mude, deleta a imagem anterior e adiciona a nova
        if (requestDto.Imagem != null)
        {
            await _arquivoLocalService.DeletarArquivo(compromisso.Imagem);
            var newImagem = await _arquivoLocalService.SalvarArquivoLocalAsync(requestDto.Imagem);
            compromisso.Imagem = newImagem == null ? compromisso.Imagem : newImagem.Path;
        }
        
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

        if (compromisso is null)
            throw new Exception("Compromisso não encontrado pelo token informado.");

        // As participações vêm carregadas com o compromisso: dá para excluí-las direto.
        foreach (var participacao in compromisso.Participacoes)
        {
            await _integranteCompromissoRepository.Delete(participacao);
        }
        await _compromissoFinanceiroRepository.Delete(compromisso);
    }
    public async Task<CompromissoFinanceiroResponseDTO> AddIntegrante(List<CreateIntegranteCompromissoRequestDTO> requestDtoList, string compromissoToken)
    {
        var compromisso = await _compromissoFinanceiroRepository.GetByToken(compromissoToken);

        foreach (var requestDto in requestDtoList)
        {
            // Quem já está na divisão deste compromisso é ignorado, não duplicado.
            var jaParticipa = compromisso.Participacoes
                .Any(p => p.Integrante != null && p.Integrante.Token == requestDto.IntegranteToken);

            if (jaParticipa)
                continue;

            var integrante = await _integranteRepository.GetByToken(requestDto.IntegranteToken);

            if (integrante is null)
                throw new Exception("Integrante não foi encontrado pelo token informado.");

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

        if (integranteCompromisso is null)
            throw new Exception("Participação não encontrada pelo token informado.");

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
        ICollection<IntegranteCompromisso>? participacoes)
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
            Imagem = _arquivoLocalService.ObterUrlLocalCompleta(compromisso.Imagem),
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

    private CompromissoFinanceiroResponseMainDTO ConvertCompromissoToResponseMainDto(
        CompromissoFinanceiro compromisso)
    {
        var valores = GetValoresCompromisso(compromisso);
        var vencimento = GetVencimento(compromisso);
        var responseDto = new CompromissoFinanceiroResponseMainDTO
        {
            CompromissoFinanceiroToken = compromisso.Token,
            GrupoToken = compromisso.Grupo.Token,
            Titulo = compromisso.Titulo,
            Imagem = _arquivoLocalService.ObterUrlLocalCompleta(compromisso.Imagem),
            SituacaoPagamento = valores.situacaoPagamento,
            DataVencimento = compromisso.Data,
            DiasParaVencimento = vencimento.diasParaVencimento,
            Vencido = vencimento.vencido,
            TipoDivisao = compromisso.TipoDivisao,
            ValorTotal = valores.valorTotal,
            ValorPagoTotal = valores.valorPagoTotal,
            ValorRestanteTotal = valores.valorRestanteTotal,
            ProgressoPagamentos = valores.ProgressoPagamentos,
            Categoria = compromisso.Categoria,
            NomeGrupo = compromisso.Grupo.Nome,
            IntegrantesQuitadosCount = valores.integrantesQuitadosCount,
            IntegrantesDevendoCount = valores.integrantesDevendoCount,
            IntegrantesTotalCount = valores.integrantesTotalCount,
            Integrantes = ConvertIntegranteToNewResponseDto(compromisso.Participacoes),
            DeletedAt = compromisso.DeletedAt
        };

        return responseDto;
    }

    private (int diasParaVencimento, bool vencido) GetVencimento(CompromissoFinanceiro compromisso)
    {
        int diasParaVencimento = 0;
        DateTime dataVencimento = compromisso.Data;
        DateTime hoje = DateTime.UtcNow;
        bool vencido = false;
        
        diasParaVencimento = (dataVencimento - hoje).Days;
        if (diasParaVencimento <= 0)
            vencido = true;
        
        return (diasParaVencimento, vencido);
    }
    private (SituacaoPagamento situacaoPagamento, decimal valorTotal, decimal valorPagoTotal, decimal valorRestanteTotal
        , float ProgressoPagamentos, int integrantesQuitadosCount, int integrantesDevendoCount, int
        integrantesTotalCount) GetValoresCompromisso(CompromissoFinanceiro compromisso)
    {
        SituacaoPagamento situacaoPagamento;
        decimal valorTotal = compromisso.ValorTotal;
        decimal valorPagoTotal = 0;
        decimal valorRestanteTotal = 0;
        float progressoPagamentos = 0;
        int integrantesQuitadosCount = 0;
        int integrantesDevendoCount = 0;
        int integrantesTotalCount = 0;
        
        foreach (var participacao in compromisso.Participacoes)
        {
            valorPagoTotal += participacao.ValorPago;

            if (participacao.ValorPago < participacao.ValorDevedor)
                integrantesDevendoCount++;
            else
                integrantesQuitadosCount++;

            integrantesTotalCount++;
        }

        
        valorRestanteTotal = (valorTotal - valorPagoTotal);
        //caso a divisão passe de 1 (em casos da soma de pagamentos ultrapassar o valor total, o valor máximo fica como 1
        progressoPagamentos = Math.Min((float)(valorPagoTotal / valorTotal), 1);

        //para facilitar o calculo da situacao, usa-se o progresso que vai de 0 a 1
        situacaoPagamento = progressoPagamentos switch
        {
            0 => SituacaoPagamento.Pendente,
            1 => SituacaoPagamento.Pago,
            _ => SituacaoPagamento.Parcial
        };

        return (situacaoPagamento, valorTotal,  valorPagoTotal, valorRestanteTotal, progressoPagamentos, integrantesQuitadosCount, integrantesDevendoCount, integrantesTotalCount);
    }

    private ICollection<IntegranteResponseFromCompromissoDTO> ConvertIntegranteToNewResponseDto(
        ICollection<IntegranteCompromisso> participacoes)
    {
        var responseList = new List<IntegranteResponseFromCompromissoDTO>();
        
        foreach (var participacao in participacoes)
        {
            var metodosDict = new Dictionary<MetodoPagamento, decimal>();
            foreach (var pagamento in participacao.Pagamentos)
            {
                metodosDict.TryAdd(pagamento.Metodo, 0);
                metodosDict[pagamento.Metodo]+= pagamento.Valor;
            }
            
            decimal valorPago = participacao.ValorPago;
            decimal valorDevedor = participacao.ValorDevedor;
            decimal valorRestante = (valorDevedor - valorPago);
            bool quitado = valorRestante <= 0;

            var responseDto = new IntegranteResponseFromCompromissoDTO
            {
                IntegranteToken = participacao.Integrante.Token,
                UsuarioToken = participacao.Integrante.Usuario.Token,
                Nome = participacao.Integrante.Usuario.Nome,
                Sobrenome = participacao.Integrante.Usuario.Sobrenome,
                Email = participacao.Integrante.Usuario.Email,
                ImagemUrl = participacao.Integrante.Usuario.Imagem,
                ValorPago = valorPago,
                ValorDevedor = valorDevedor,
                ValorRestante = valorRestante,
                SomaPagamentosPorMetodo = metodosDict,
                Quitado = quitado,
                DeletedAt = participacao.DeletedAt
            };
            responseList.Add(responseDto);
        }
        
        return responseList;
    }
    #endregion
    
}