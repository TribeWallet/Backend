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
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
    {
        try
        {
            var usuario = await _usuarioService.Login(loginRequest);
            return Ok(usuario);

        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(e.Message);
        }

    }

    [HttpPost("register")]
    public async Task<IActionResult> Signup([FromBody] CreateUsuarioRequestDTO createUsuarioRequestDto)
    {
        try
        {
            var responseDto = await _usuarioService.Create(createUsuarioRequestDto);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}