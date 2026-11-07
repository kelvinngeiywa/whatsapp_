using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using whatsapp_cloud_api.Services;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController(ILogger<MessagingController> logger,IConfiguration configuration, IHubSignature hubSignature,
        IWhatsAppMessage whatsAppMessage,
        IMessageProcessing messageProcessing) : ControllerBase
    {
        private readonly string webhookToken = configuration["ConnectionStrings:Webhook_Verification_token"];

        private bool SecureCompare(string a, string b)
        {
            if(a.Length != b.Length)
            {
                return false;
            }
            return a.Zip(b, (c1, c2) => c1 == c2).All(x=>x);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            //Enable buffering so we can read the stream multiple times
            Request.EnableBuffering();

            //Calculate x-hub signature value to check with value in request header
            var calculateXHubSignature = await hubSignature.SignAsync(Request.Body);
            logger.LogWarning($"Calculated X-Hub-Signature: ***");

            //Reset the request body stream position
            Request.Body.Position = 0;

            var receivedSignature = Request.Headers["x-hub-signature-256"].ToString();
            logger.LogWarning($"Received X-Hub-Signature: ----");

            if (string.IsNullOrEmpty(receivedSignature) || receivedSignature != calculateXHubSignature)
            {
                Console.WriteLine("Warning - request header X-Hub-Signature not present or invalid");
                return Unauthorized();
            }

            var requestBody=await JsonDocument.ParseAsync(Request.Body);
            var body = requestBody.RootElement.GetProperty("entry")[0].GetProperty("changes")[0];

            if(body.GetProperty("field").GetString() != "messages")
            {
                return BadRequest();
            }

            var value = body.GetProperty("value");

            if(value.TryGetProperty("messages", out var messages) && value.TryGetProperty("contacts",out var contacts))
            {
                if(messages.ValueKind == JsonValueKind.Array && contacts.ValueKind == JsonValueKind.Array)
                {
                    var messageTasks = messages.EnumerateArray().Zip(contacts.EnumerateArray(),
                        async (message, contact) => await Task.WhenAll(
                            messageProcessing.ProcessMesssage(message,contact)
                            ));

                    await Task.WhenAll(messageTasks);
                }

                var sendStatus = new { 
                    messaging_product="whatsapp",
                    status="read",
                    message_id = messages[0].GetProperty("id").GetString()
                };

                try
                {
                    await whatsAppMessage.SendMessage(sendStatus);
                }
                catch(Exception exception)
                {
                    Console.Error.WriteLine($"Error while marking message as read (TNZ): {exception.Message}");
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name ="hub.mode")]string mode,[FromQuery(Name ="hub.verify_token")] string verifyToken,
            [FromQuery(Name ="hub.challenge")] string challenge)
        {
            if(mode =="subscribe" && verifyToken == webhookToken)
            {
                return Ok(challenge);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("greetings")]
        public IActionResult Greetings()
        {
            return Ok("Good afternoon!!!");

        }
    }
}
