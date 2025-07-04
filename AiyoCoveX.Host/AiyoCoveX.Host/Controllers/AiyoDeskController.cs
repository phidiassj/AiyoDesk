using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiyoCoveX.Host.Controllers;

[Route("[controller]")]
[EnableCors("AllowAllHeaders")]
[ApiController]
public class AiyoDeskController : ControllerBase
{
    private readonly IHttpContextAccessor httpContext;

    public AiyoDeskController(IHttpContextAccessor httpContext)
    {
        this.httpContext = httpContext;
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok();
    }

}
