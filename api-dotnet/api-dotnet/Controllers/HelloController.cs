using Microsoft.AspNetCore.Mvc;

namespace api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        // GET: api/hello
        [HttpGet]
        public IActionResult SayHello()
        {
            return Ok("Hello World from .NET 9!");
        }
    }
}