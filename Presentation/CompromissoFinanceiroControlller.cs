using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Compromisso.DTOs;
using TribeWallet.Application.CompromissoFinanceiro;
using TribeWallet.Application.IntegranteCompromisso.DTOs;

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

    [HttpGet("integrantes/{integranteToken}")]
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

    [HttpGet("{grupoToken}")]
    public async Task<IActionResult> GetAllByGrupoToken(string grupoToken, bool deleted)
    {
        try
        {
            var responseDto = await _compromissoFinanceiroService.GetAllByGrupoToken(grupoToken,deleted);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("{grupoToken}")]
    public async Task<IActionResult> CreateCompromissoFinanceiro([FromBody] CreateCompromissoFinanceiroRequestDTO compromissoFinanceiroRequestDto, string grupoToken)
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

    [HttpPost("{compromissoToken}/integrantes")]
    public async Task<IActionResult> AddIntegrantes([FromBody] List<CreateIntegranteCompromissoRequestDTO> requestDto, string compromissoToken)
    {
        try
        {
            var responseDto = await _compromissoFinanceiroService.AddIntegrante(requestDto, compromissoToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("integrantes/{integranteCompromissoToken}")]
    public async Task<IActionResult> RemoveIntegrantes(string integranteCompromissoToken)
    {
        try
        {
            await _compromissoFinanceiroService.RemoveIntegrante(integranteCompromissoToken);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{compromissoToken}")]
    public async Task<IActionResult> DeleteCompromissoFinanceiro(string compromissoToken)
    {
        try
        {
            await _compromissoFinanceiroService.DeleteCompromissoFinanceiro(compromissoToken);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}