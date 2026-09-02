using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Presentation;
[Route("api/usuarios")]
[ApiController]
public class UsuarioController: ControllerBase
{
    private readonly UsuarioService _service;
    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _service.GetAll();
        return Ok(usuarios);
    }

    [Authorize]
    [HttpPut("{usuarioToken}")]
    public async Task<IActionResult> EditUsuario([FromBody] EditUsuarioDTO editUsuarioDto, string usuarioToken)
    {
        try
        {
            var responseDto =  await _service.Update(editUsuarioDto, usuarioToken);
            return  Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}