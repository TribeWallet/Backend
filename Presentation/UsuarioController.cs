using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;
using TribeWallet.Application;
using TribeWallet.Domain;

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
        _service.GetAll();
        return Ok();
    }

    [HttpGet("{usuarioId}")]
    public IActionResult GetById(int usuarioId)
    {
        var usuario = _service.GetById(usuarioId);
        var returnDto = new ReturnUsuarioDTO(
            usuarioId: usuario.UsuarioId,
            nome: usuario.Nome,
            sobrenome: usuario.Sobrenome,
            email: usuario.Email,
            username: usuario.Username
        );
        
        return Ok(returnDto);
    }

    [HttpPost("signup")]
    public IActionResult SignUp(CreateUsuarioDTO createUsuarioDto)
    {
        var usuario = new Usuario(
            nome: createUsuarioDto.Nome,
            sobrenome: createUsuarioDto.Sobrenome,
            email: createUsuarioDto.Username,
            username: createUsuarioDto.Email,
            hashSenha: createUsuarioDto.Senha);
        
        usuario = _service.Create(usuario);
        var returnDto = new ReturnUsuarioDTO(
            usuarioId: usuario.UsuarioId,
            nome: usuario.Nome,
            sobrenome: usuario.Sobrenome,
            email: usuario.Email,
            username: usuario.Username);
        
        return Created("api/users", returnDto);
    }

    [HttpPost("/login")]
    public IActionResult Login([FromBody] LoginDTO loginDto)
    {
        var usuario = _service.Login(loginDto);
        var returnDto = new ReturnUsuarioDTO(
            usuarioId: usuario.UsuarioId,
            nome: usuario.Nome,
            sobrenome: usuario.Sobrenome,
            email: usuario.Email,
            username: usuario.Username);
        
        return Ok(returnDto);
    }
}