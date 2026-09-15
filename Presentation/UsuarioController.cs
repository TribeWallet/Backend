using System.Diagnostics.CodeAnalysis;
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
    private readonly UsuarioService _usuarioService;
    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(bool deleted = false)
    {
        var usuarios = await _usuarioService.GetAll(deleted);
        return Ok(usuarios);
    }

    [Authorize]
    [HttpDelete("{usuarioToken}")]
    public async Task<IActionResult> DeleteUsuario(string usuarioToken)
    {
        try
        {
            await _usuarioService.Delete(usuarioToken);
            return NoContent();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpPut("{usuarioToken}")]
    public async Task<IActionResult> UpdateUsuario([FromBody] EditUsuarioDTO editUsuarioDto, string usuarioToken)
    {
        try
        {
            var responseDto =  await _usuarioService.Update(editUsuarioDto, usuarioToken);
            return  Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpGet("{nome}")]
    public async Task<IActionResult> GetUsuarioByNome(string nome)
    {
        try
        {
            var responseDto = await _usuarioService.GetByNome(nome);
            return  Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}