using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Application.Usuario;

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
    public async Task<IActionResult> Register([FromBody] CreateUsuarioRequestDTO createUsuarioRequestDto)
    {
        try
        {
            var responseDto = await _usuarioService.Create(createUsuarioRequestDto);
            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(new { mensagem = e.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EsqueceuSenhaRequestDTO requestDto)
    {
        try
        {
            await _usuarioService.SolicitarRecuperacaoSenha(requestDto);
            return Ok(new { mensagem = "Se o e-mail informado estiver cadastrado, um código de recuperação foi enviado." });
        }
        catch (Exception e)
        {
            return BadRequest(new { mensagem = e.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetSenhaRequestDTO requestDto)
    {
        try
        {
            await _usuarioService.RedefinirSenha(requestDto);
            return Ok(new { mensagem = "Senha redefinida com sucesso." });
        }
        catch (Exception e)
        {
            return BadRequest(new { mensagem = e.Message });
        }
    }
}