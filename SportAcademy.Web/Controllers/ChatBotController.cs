using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        [HttpPost]
        public IActionResult GetResponse([FromBody] string userInput)
        {
            string botResponse = $"You said: {userInput}. This is a response from the chatbot.";
            return Ok(botResponse);
        }

    }
}
