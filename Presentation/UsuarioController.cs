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

    /*
    [HttpGet("{usuarioId}")]
    public IActionResult GetById(Guid usuarioId)
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
    */

    /*[HttpPost("signup")]
    public IActionResult SignUp(CreateUsuarioDTO createUsuarioDto)
    {
        var usuario = new Usuario
        {
            Nome =  createUsuarioDto.Nome,
            Sobrenome = createUsuarioDto.Sobrenome,
            Email = createUsuarioDto.Email,
            Username = createUsuarioDto.Username,
            Imagem = createUsuarioDto.Imagem,
            HashSenha =  createUsuarioDto.Senha
        };
        
        usuario = _service.Create(usuario);
        var returnDto = new ReturnUsuarioDTO(
            usuarioId: usuario.UsuarioId,
            nome: usuario.Nome,
            sobrenome: usuario.Sobrenome,
            email: usuario.Email,
            username: usuario.Username);
        
        return Created("api/users", returnDto);
    }*/
}