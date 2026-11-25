using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Notificacoes;

namespace TribeWallet.Presentation;

[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacaoController : ControllerBase
{
    private readonly NotificacaoService _notificacaoService;

    public NotificacaoController(NotificacaoService notificacaoService)
    {
        _notificacaoService = notificacaoService;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Notificações do usuário logado, da mais recente para a mais antiga.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await _notificacaoService.Listar(UsuarioId));

    /// <summary>Quantas ainda não foram lidas; alimenta o número do sininho.</summary>
    [HttpGet("nao-lidas/contagem")]
    public async Task<IActionResult> ContarNaoLidas() => Ok(await _notificacaoService.ContarNaoLidas(UsuarioId));

    [HttpPatch("lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        await _notificacaoService.MarcarTodasComoLidas(UsuarioId);
        return NoContent();
    }

    [HttpPatch("{token}/lida")]
    public async Task<IActionResult> MarcarComoLida(string token)
    {
        try
        {
            await _notificacaoService.MarcarComoLida(token, UsuarioId);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { mensagem = e.Message });
        }
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Excluir(string token)
    {
        try
        {
            await _notificacaoService.Excluir(token, UsuarioId);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { mensagem = e.Message });
        }
    }
}
