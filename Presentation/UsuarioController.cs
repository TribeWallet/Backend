using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Domain;
using TribeWallet.Domain.Entities;

namespace TribeWallet.Presentation;
[Route("api/users")]
[ApiController]
public class UsuarioController: ControllerBase
{
    private readonly UsuarioService _service;
    public UsuarioController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var usuarios = _service.GetAll();
        return Ok(usuarios);
    }

    [HttpPut("{token}")]
    public IActionResult EditUsuario([FromBody] EditUsuarioDTO editUsuarioDto)
    {
        try
        {
            var returnDto =  _service.Update(editUsuarioDto);
            return  Ok(returnDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}