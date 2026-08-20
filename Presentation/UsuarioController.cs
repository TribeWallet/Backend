using Microsoft.AspNetCore.Mvc;

namespace TribeWallet.Presentation;
[Route("api/user")]
[ApiController]
public class UsuarioController: ControllerBase
{

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok("Hello World!");
    }
}