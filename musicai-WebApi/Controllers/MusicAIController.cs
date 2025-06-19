using Microsoft.AspNetCore.Mvc;

namespace musicai.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicAIController : ControllerBase
{
  

    [HttpGet(Name = "OPEN-IA")]
    public string  Get()
    {
        return "Hello, this is the OpenAI API!";            
    }
}