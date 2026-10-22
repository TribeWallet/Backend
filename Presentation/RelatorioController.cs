using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Relatorio;

namespace TribeWallet.Presentation;


[ApiController]
[Route("api/relatorio")]
[Authorize] // Exige que o usuário esteja logado
public class RelatorioController : ControllerBase
{
    private readonly RelatorioService _relatorioService;

    public RelatorioController(RelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpGet("/usuario/{usuarioToken}")]
    public async Task<IActionResult> GetByUsuarioToken(string usuarioToken)
    {
        try
        {
            var relatoriobytes = await _relatorioService.GetByUserToken(usuarioToken);
            var relatorioPdf = File(relatoriobytes, "application/pdf", $"relatorio-{usuarioToken}.pdf");
            return relatorioPdf;
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}