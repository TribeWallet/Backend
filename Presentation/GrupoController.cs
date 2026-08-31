using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application.Grupo;

namespace TribeWallet.Presentation;

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
            var returnDto = await _service.GetByUsuarioToken(usuarioToken);
            return Ok(returnDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}