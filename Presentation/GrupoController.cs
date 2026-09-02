using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Grupo;
using TribeWallet.Application.Grupo.DTOs;

namespace TribeWallet.Presentation;

[Route("api/grupos")]
[ApiController]
[Authorize]
public class GrupoController : ControllerBase
{
    private readonly GrupoService _service;

    public GrupoController(GrupoService service)
    {
        _service = service;
    }

    [HttpGet("{usuarioToken}")]
    public async Task<IActionResult> GetByUsuarioToken(string usuarioToken)
    {
        try
        {
            var responseDto = await _service.GetAllByUsuarioToken(usuarioToken);
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
            var responseDto = await _service.Create(requestDto);
            return Ok(responseDto);
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
            var responseDto = await _service.Update(requestDto, grupoToken);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}