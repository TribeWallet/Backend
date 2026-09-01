using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _service.GetAll();
        return Ok(usuarios);
    }

    [Authorize]
    [HttpPut("{token}")]
    public async Task<IActionResult> EditUsuario([FromBody] EditUsuarioDTO editUsuarioDto, string token)
    {
        try
        {
            var returnDto =  await _service.Update(editUsuarioDto, token);
            
            //busca token jwt dos headers da requisição
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var jwtToken = authHeader.StartsWith("Bearer ") 
                ? authHeader.Substring("Bearer ".Length) 
                : authHeader;
            
            returnDto.JwtToken = jwtToken;
            return  Ok(returnDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}