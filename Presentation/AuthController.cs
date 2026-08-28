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
    public IActionResult Login([FromBody] LoginDTO login)
    {
        try
        {
            var usuario = _usuarioService.Login(login);
            return Ok(usuario);

        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }

    }

    [HttpPost("register")]
    public IActionResult Signup([FromBody] RegisterUsuarioDTO registerUsuarioDto)
    {
        try
        {
            var returnUsuarioDto = _usuarioService.Create(registerUsuarioDto);
            return Ok(returnUsuarioDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}