using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    public IActionResult Add(CreateUsuarioDTO createUsuarioDto)
    {
        /*
        Usuario usuario = new Usuario(
        {
            Nome = createUsuarioDto.Nome,
            Sobrenome = createUsuarioDto.Sobrenome,
            Email = createUsuarioDto.Email,
            Username = createUsuarioDto.Username,
            HashSenha = createUsuarioDto.Senha
        });
         */
        
        var usuario = new Usuario(nome: createUsuarioDto.Nome,
            sobrenome: createUsuarioDto.Sobrenome,
            email: createUsuarioDto.Username,
            username: createUsuarioDto.Email,
            hashSenha: createUsuarioDto.Senha);
        
        usuario = _service.Create(usuario);
        var returnDTO = new ReturnUsuarioDTO(usuarioId: usuario.UsuarioId,
            nome: usuario.Nome,
            sobrenome: usuario.Sobrenome,
            email: usuario.Email,
            username: usuario.Username);
        
        return Created("api/users", returnDTO);
    }
}