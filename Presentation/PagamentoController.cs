using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Pagamento;
using TribeWallet.Application.Pagamento.DTOs;

namespace TribeWallet.Presentation;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Exige que o usuário esteja logado
public class PagamentoController : ControllerBase
{
    private readonly PagamentoService _pagamentoService;

    public PagamentoController(PagamentoService pagamentoService)
    {
        _pagamentoService = pagamentoService;
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarPagamento([FromBody] CreatePagamentoRequestDTO dto)
    {
        try
        {
            var result = await _pagamentoService.RegistrarPagamento(dto);
            return CreatedAtAction(nameof(RegistrarPagamento), new { token = result.PagamentoToken }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro interno no servidor.", detalhe = ex.Message });
        }
    }

    [HttpPut("{token}")]
    public async Task<IActionResult> EditarPagamento(string token, [FromBody] UpdatePagamentoRequestDTO dto)
    {
        try
        {
            var result = await _pagamentoService.EditarPagamento(token, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            if (ex.Message == "Pagamento não encontrado.")
                return NotFound(new { mensagem = ex.Message });

            return StatusCode(500, new { mensagem = "Erro interno no servidor.", detalhe = ex.Message });
        }
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> ExcluirPagamento(string token)
    {
        try
        {
            await _pagamentoService.ExcluirPagamento(token);
            return NoContent(); // 204 No Content é o padrão para deletes bem-sucedidos
        }
        catch (Exception ex)
        {
            if (ex.Message == "Pagamento não encontrado.")
                return NotFound(new { mensagem = ex.Message });

            return StatusCode(500, new { mensagem = "Erro interno no servidor.", detalhe = ex.Message });
        }
    }
}