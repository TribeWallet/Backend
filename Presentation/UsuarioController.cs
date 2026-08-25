using Microsoft.AspNetCore.Mvc;
using TribeWallet.Application;

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
}