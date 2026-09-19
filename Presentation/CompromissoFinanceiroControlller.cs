using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.CompromissoFinanceiro;

namespace TribeWallet.Presentation;


[Route("api/compromissos")]
[ApiController]
[Authorize]
public class CompromissoFinanceiroControlller : ControllerBase
{
 
    private readonly CompromissoFinanceiroService _compromissoFinanceiroService;

    public CompromissoFinanceiroControlller(CompromissoFinanceiroService compromissoFinanceiroService)
    {
        _compromissoFinanceiroService = compromissoFinanceiroService;
    }

    [HttpGet("{integranteToken}")]
    public async Task<IActionResult> GetAllByIntegranteToken(string integranteToken)
    {
        try
        {
            var responseDto = await _compromissoFinanceiroService.GetAllByIntegranteToken(integranteToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}