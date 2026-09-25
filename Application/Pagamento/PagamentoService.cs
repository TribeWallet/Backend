using TribeWallet.Application.Pagamento.DTOs;
using TribeWallet.Application.IntegranteCompromisso; 
using TribeWallet.Application.IntegranteCompromisso.DTOs; 
using TribeWallet.Application.Integrante;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application; // Namespace do UsuarioResponseDTO[cite: 14]
using TribeWallet.Application.Grupo.DTOs; // Namespace do GrupoResponseDTO[cite: 13]
using TribeWallet.Domain.Entities;

namespace TribeWallet.Application.Pagamento;

public class PagamentoService
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IIntegranteCompromissoRepository _integranteCompromissoRepository; 

    public PagamentoService(IPagamentoRepository pagamentoRepository, IIntegranteCompromissoRepository integranteCompromissoRepository)
    {
        _pagamentoRepository = pagamentoRepository;
        _integranteCompromissoRepository = integranteCompromissoRepository;
    }

    public async Task<PagamentoResponseDTO> RegistrarPagamento(CreatePagamentoRequestDTO dto)
    {
        if (dto.Valor <= 0)
            throw new ArgumentException("O valor do pagamento deve ser maior que zero.");

        var integranteCompromisso = await _integranteCompromissoRepository.GetByToken(dto.IntegranteCompromissoToken);
        if (integranteCompromisso == null)
            throw new ArgumentException("Fatia de compromisso não encontrada.");

        var pagamento = new Domain.Entities.Pagamento
        {
            IntegranteCompromissoId = integranteCompromisso.IntegranteCompromissoId,
            Valor = dto.Valor,
            Data = dto.Data,
            Metodo = (MetodoPagamento)dto.Metodo, 
            ComprovanteUrl = dto.ComprovanteBase64, 
            IntegranteCompromisso = integranteCompromisso 
        };

        var novoPagamento = await _pagamentoRepository.Add(pagamento);
        
        integranteCompromisso.ValorPago = novoPagamento.Valor;
        await _integranteCompromissoRepository.Update(integranteCompromisso);
        // Ao salvar, o EF pode não retornar a árvore completa de dependências na mesma instância.
        // Recarregar pelo Token garante que o repositório aplique os .Include() definidos e 
        // o MapToResponseDTO tenha todos os dados necessários.
        var pagamentoCompleto = await _pagamentoRepository.GetByToken(novoPagamento.Token);
        
        return MapToResponseDTO(pagamentoCompleto ?? novoPagamento);
    }

    public async Task<PagamentoResponseDTO> EditarPagamento(string pagamentoToken, UpdatePagamentoRequestDTO dto)
    {
        var pagamento = await _pagamentoRepository.GetByToken(pagamentoToken);
        
        if (pagamento == null)
            throw new Exception("Pagamento não encontrado.");

        if (dto.Valor.HasValue)
        {
            if (dto.Valor.Value <= 0)
                throw new ArgumentException("O valor do pagamento deve ser maior que zero.");
            
            pagamento.Valor = dto.Valor.Value;
        }

        if (dto.Data.HasValue)
            pagamento.Data = dto.Data.Value;

        if (dto.Metodo.HasValue)
            pagamento.Metodo = (MetodoPagamento)dto.Metodo.Value;

        if (!string.IsNullOrEmpty(dto.ComprovanteBase64))
            pagamento.ComprovanteUrl = dto.ComprovanteBase64;

        var pagamentoAtualizado = await _pagamentoRepository.Update(pagamento);

        return MapToResponseDTO(pagamentoAtualizado);
    }

    public async Task ExcluirPagamento(string pagamentoToken)
    {
        var pagamento = await _pagamentoRepository.GetByToken(pagamentoToken);
        
        if (pagamento == null)
            throw new Exception("Pagamento não encontrado.");

        await _pagamentoRepository.Delete(pagamento);
    }

    public async Task<ICollection<PagamentoResponseDTO>> GetPagamentosByIntegranteCompromissoToken(string integranteCompromissoToken)
    {
        var pagamentos = await _pagamentoRepository.GetAllByIntegranteCompromissoToken(integranteCompromissoToken);
        var responseDtoList = new List<PagamentoResponseDTO>();

        foreach (var pagamento in pagamentos)
        {
            var reponseDto = MapToResponseDTO(pagamento);
            responseDtoList.Add(reponseDto);
        }

        return responseDtoList;
    }

    private PagamentoResponseDTO MapToResponseDTO(Domain.Entities.Pagamento pagamento)
    {
        var response = new PagamentoResponseDTO
        {
            PagamentoToken = pagamento.Token,
            Valor = pagamento.Valor,
            Data = pagamento.Data,
            ComprovanteUrl = pagamento.ComprovanteUrl ?? string.Empty,
            Metodo = pagamento.Metodo
        };

        if (pagamento.IntegranteCompromisso != null)
        {
            response.IntegranteCompromisso = new IntegranteCompromissoResponseDTO
            {
                IntegranteCompromissoToken = pagamento.IntegranteCompromisso.Token,
                ValorDevedor = pagamento.IntegranteCompromisso.ValorDevedor,
                ValorPago = pagamento.IntegranteCompromisso.ValorPago,
            };

            // Mapeia o Integrante se estiver carregado
            if (pagamento.IntegranteCompromisso.Integrante != null)
            {
                var integrante = pagamento.IntegranteCompromisso.Integrante;
                
                response.IntegranteCompromisso.Integrante = new IntegranteResponseDTO
                {
                    IntegranteToken = integrante.Token,
                    DeletedAt = integrante.DeletedAt,
                    // Assume-se que a Entidade Integrante possua a propriedade Grupo com seu Token
                    GrupoToken = integrante.Grupo?.Token ?? string.Empty 
                };

                // Mapeia o Usuario[cite: 14]
                if (integrante.Usuario != null)
                {
                    response.IntegranteCompromisso.Integrante.Usuario = new UsuarioResponseDTO
                    {
                        UsuarioToken = integrante.Usuario.Token,
                        Nome = integrante.Usuario.Nome,
                        Sobrenome = integrante.Usuario.Sobrenome,
                        Email = integrante.Usuario.Email,
                        Username = integrante.Usuario.Username,
                        DeletedAt = integrante.Usuario.DeletedAt
                    };
                }
            }

            // Mapeia o CompromissoFinanceiro se estiver carregado
            if (pagamento.IntegranteCompromisso.Compromisso != null)
            {
                var compromisso = pagamento.IntegranteCompromisso.Compromisso;
                
                response.IntegranteCompromisso.CompromissoFinanceiro = new CompromissoFinanceiroResponseDTO
                {
                    CompromissoFinanceiroToken = compromisso.Token,
                    Titulo = compromisso.Titulo, 
                    ValorTotal = compromisso.ValorTotal,
                    Data = compromisso.Data,
                    TipoDivisao = compromisso.TipoDivisao,
                    Imagem = compromisso.Imagem,
                    Categoria = compromisso.Categoria
                };

                // Mapeia o Grupo[cite: 13]
                if (compromisso.Grupo != null)
                {
                    response.IntegranteCompromisso.CompromissoFinanceiro.Grupo = new GrupoResponseDTO
                    {
                        GrupoToken = compromisso.Grupo.Token,
                        Nome = compromisso.Grupo.Nome,
                        Descricao = compromisso.Grupo.Descricao,
                        DeletedAt = compromisso.Grupo.DeletedAt
                    };
                }
            }
        }

        return response;
    }
}