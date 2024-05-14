using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
    [HttpPost("{token}")]
    public async Task<IActionResult> GetOAuthToken(string token)
    {
        return Ok(token);
    }
}