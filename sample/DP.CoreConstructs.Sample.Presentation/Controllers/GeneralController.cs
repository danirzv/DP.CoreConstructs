using DP.CoreConstructs.Sample.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DP.CoreConstructs.Sample.Presentation.Controllers;

[ApiController]
[Route("api")]
public class GeneralController : Controller
{
    [HttpGet("Test")]
    public string Test([FromQuery] PhoneNumber pa)
    {
        return pa.Value;
    }
    
    [HttpPost("Test")]
    public string Test2(PhoneNumber pa)
    {
        return pa.Value;
    }
}