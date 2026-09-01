using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;
using TribeWallet.Services;

namespace TribeWallet.Presentation;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    public AuthController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO login)
    {
        try
        {
            var usuario = await _usuarioService.Login(login);
            return Ok(usuario);

        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }

    }

    [HttpPost("register")]
    public async Task<IActionResult> Signup([FromBody] UsuarioRegisterDTO usuarioRegisterDto)
    {
        try
        {
            var returnUsuarioDto = await _usuarioService.Create(usuarioRegisterDto);
            return Ok(returnUsuarioDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}