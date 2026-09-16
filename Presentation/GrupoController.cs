using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Grupo.DTOs;
using TribeWallet.Application.Integrante;
using TribeWallet.Infrastructure;

namespace TribeWallet.Presentation;

[Route("api/grupos")]
[ApiController]
[Authorize]
public class GrupoController : ControllerBase
{
    private readonly GrupoService _grupoService;
    private readonly IntegranteService _integranteService;

    public GrupoController(GrupoService grupoService, IntegranteService integranteService)
    {
        _grupoService = grupoService;
        _integranteService = integranteService;
    }

    [HttpGet("{usuarioToken}")]
    public async Task<IActionResult> GetByUsuarioToken(string usuarioToken, bool deleted)
    {
        try
        {
            var responseDto = await _grupoService.GetAllByUsuarioToken(usuarioToken, deleted);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateGrupo([FromBody] CreateGrupoRequestDTO requestDto)
    {
        try
        {
            var responseDto = await _grupoService.Create(requestDto);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{grupoToken}")]
    public async Task<IActionResult> DeleteGrupo(string grupoToken)
    {
        try
        {
            await _grupoService.Delete(grupoToken);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{grupoToken}")]
    public async Task<IActionResult> UpdateGrupo([FromBody] UpdateGrupoRequestDTO requestDto, string  grupoToken)
    {
        try
        {
            var responseDto = await _grupoService.Update(requestDto, grupoToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPut("{grupoToken}/integrantes")]
    public async Task<IActionResult> AddIntegrantes([FromBody] List<CreateIntegranteRequestDTO> requestDto, string grupoToken)
    {
        try
        {
            var responseDto = await _integranteService.AddIntegranteToGrupo(requestDto, grupoToken);
            return Ok(responseDto);
        } catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{grupoToken}/integrantes/{integranteToken}")]
    public async Task<IActionResult> RemoveIntegrante(string grupoToken, string integranteToken)
    {
        try
        {
            var responseDeto = await _grupoService.RemoveIntegrante(grupoToken, integranteToken);
            return  Ok(responseDeto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}