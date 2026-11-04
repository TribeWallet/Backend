using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Relatorio;

namespace TribeWallet.Presentation;


[ApiController]
[Route("api/relatorios")]
[Authorize] // Exige que o usuário esteja logado
public class RelatorioController : ControllerBase
{
    private readonly RelatorioService _relatorioService;

    public RelatorioController(RelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpGet("usuario/{usuarioToken}")]
    public async Task<IActionResult> GetByUsarioToken(string usuarioToken)
    {
        try
        {
            var relatorio = await _relatorioService.GetByUsuarioToken(usuarioToken);
            return Ok(relatorio);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    [HttpGet("usuario/{usuarioToken}/pdf")]
    public async Task<IActionResult> GenPdfForUsuario(string usuarioToken)
    {
        try
        {
            var relatoriobytes = await _relatorioService.GenUsuarioPdf(usuarioToken);
            var relatorioPdf = File(relatoriobytes, "application/pdf", $"relatorio-{usuarioToken}.pdf");
            return relatorioPdf;
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("usuario/{usuarioToken}/grupo/{grupoToken}")]
    public async Task<IActionResult> GetByGrupoToken(string usuarioToken, string grupoToken)
    {
        try
        {
            var relatorio = await _relatorioService.GetByIntegranteToken(usuarioToken, grupoToken);
            return Ok(relatorio);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}