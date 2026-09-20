using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Compromisso.DTOs;
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

    /*[HttpGet("integrantes/{integranteToken}")]
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
    }*/

    [HttpGet("{grupoToken}")]
    public async Task<IActionResult> GetAllByGrupoToken(string grupoToken)
    {
        try
        {
            var responseDto = await _compromissoFinanceiroService.GetAllByGrupoToken(grupoToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("{grupoToken}")]
    public async Task<IActionResult> AddCompromisso([FromBody] CreateCompromissoFinanceiroRequestDTO compromissoFinanceiroRequestDto, string grupoToken)
    {
        try
        {
            var responseDto = await _compromissoFinanceiroService.CreateCompromissoFinanceiro(compromissoFinanceiroRequestDto, grupoToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}