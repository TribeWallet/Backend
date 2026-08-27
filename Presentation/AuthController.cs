using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Domain.Entities;
using TribeWallet.Services;

namespace TribeWallet.Presentation;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO login)
    {
        if (login is { Email: "teste", Password: "123" })
        {
            var usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Username = "teste",
                Email = login.Email,
            };

            var token = _tokenService.GenerateToken(usuario);
            return Ok(new { token });
        }

        return Unauthorized("Credenciais inválidas");
    }
}