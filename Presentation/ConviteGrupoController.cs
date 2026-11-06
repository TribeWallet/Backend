using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.ConviteGrupo;
using TribeWallet.Application.ConviteGrupo.DTOs;

namespace TribeWallet.Presentation;

[ApiController]
[Authorize]
public class ConviteGrupoController : ControllerBase
{
    private readonly ConviteGrupoService _conviteService;

    public ConviteGrupoController(ConviteGrupoService conviteService)
    {
        _conviteService = conviteService;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Gera um link de convite para o grupo; se vier e-mail, também envia o link para ele.</summary>
    [HttpPost("api/grupos/{grupoToken}/convites")]
    public async Task<IActionResult> Criar(string grupoToken, [FromBody] CreateConviteGrupoRequestDTO? dto)
    {
        try
        {
            return Ok(await _conviteService.Criar(grupoToken, UsuarioId, dto ?? new CreateConviteGrupoRequestDTO()));
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>Mostra o grupo do convite. Público, para a tela de boas-vindas abrir antes do login.</summary>
    [AllowAnonymous]
    [HttpGet("api/convites/{conviteToken}")]
    public async Task<IActionResult> Consultar(string conviteToken)
    {
        try
        {
            return Ok(await _conviteService.Consultar(conviteToken));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>Entra no grupo do convite com o usuário logado.</summary>
    [HttpPost("api/convites/{conviteToken}/aceitar")]
    public async Task<IActionResult> Aceitar(string conviteToken)
    {
        try
        {
            var grupoToken = await _conviteService.Aceitar(conviteToken, UsuarioId);
            return Ok(new { grupoToken });
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
