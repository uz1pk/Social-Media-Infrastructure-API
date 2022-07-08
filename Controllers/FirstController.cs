using Microsoft.AspNetCore.Mvc;

namespace TweetAPI.Controllers
{
    public class FirstController : Controller
    {
        [HttpGet("api/user")]
        public IActionResult Get()
        {
            return Ok(new { name = "Hello Word" });
        }
    }
}
