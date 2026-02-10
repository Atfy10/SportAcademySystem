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
            // Here you would typically call your chatbot service to get a response based on the user input.
            // For demonstration purposes, we'll return a simple response.
            string botResponse = $"You said: {userInput}. This is a response from the chatbot.";
            return Ok(botResponse);
        }

    }
}
