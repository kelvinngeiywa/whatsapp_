using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MpesaCallbackController(HttpClient _httpClient,IConfiguration _configuration,IMpesa mpesaServices,IWhatsAppMessage whatsAppMessage,
        ILogger<MpesaCallbackController> _logger) : ControllerBase
    {

        [HttpPost("callback")]
        public async Task<IActionResult> HandleSTKCallback([FromBody] JsonElement callbackData)
        {
            var stkCallback = callbackData.GetProperty("Body").GetProperty("stkCallback");

            int resultCode = stkCallback.GetProperty("ResultCode").GetInt32();

            if(resultCode == 0)
            {
                var items = stkCallback.GetProperty("CallbackMetadata").GetProperty("Item");

                _logger.LogWarning("STK callback items: {items}",items);

                var phone = GetItemValue(items,"PhoneNumber").ToString();
                _logger.LogWarning("Phone is: ", phone);
                
                //var successMessage= new
                //{
                //    messaging_product = "whatsapp",
                //    to = phone,
                //    type = "text",
                //    text = new { body = reply }
                //}

                //await whatsAppMessage.SendMessage(items);

            }
            else
            {
                string resultDesc =stkCallback.GetProperty("ResultDesc").GetString()!;
                _logger.LogWarning("Not successful",resultDesc);
            }

            var response = new
            {
                resultCode=0,
                ResultDesc="Accepted"
            };

            return Ok(response);
        }

        [HttpGet("test-stk")]
        public async Task<IActionResult> TestStkPush([FromBody] string phone)
        {
            string callBackUrl = "https://winning-corgi-greatly.ngrok-free.app";

            var result = await mpesaServices.InitiateSTKPushAsync(phone,2,callBackUrl);
            _logger.LogError($"Results are {result}");
            return Ok(result);
        }

        private static JsonElement? GetItemValue(JsonElement items,string name)
        {
            foreach (var item in items.EnumerateArray())
            {
                if(item.GetProperty("Name").GetString()== name && item.TryGetProperty("Value", out var value))
                {
                    return value;
                }
            }
            return null;
        }
    }
}
