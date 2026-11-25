using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using whatsapp_cloud_api.Models;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MpesaCallbackController(HttpClient _httpClient,IConfiguration _configuration,IMpesa mpesaServices,IWhatsAppMessage whatsAppMessage,
        ILogger<MpesaCallbackController> _logger, IPendingPaymentStore pendingPayment) : ControllerBase
    {

        [HttpPost("callback")]
        public async Task<IActionResult> HandleSTKCallback([FromBody] MpesaCallBack payload)
        {
            //var stkCallback = callbackData.GetProperty("Body").GetProperty("stkCallback");

            //int resultCode = stkCallback.GetProperty("ResultCode").GetInt32();

            var mpesaCallback = payload?.Body?.StkCallback ;

            if (mpesaCallback is null)
            {
                return Ok(new {ResultCode=0,ResultDesc = "Accepted"});
            }

            _logger.LogInformation("Mpesa callback {Id}:{Code} {Desc}",mpesaCallback.CheckoutRequestID,mpesaCallback.ResultCode,
                mpesaCallback.ResultDesc);


            if(!pendingPayment.Tryremove(mpesaCallback.CheckoutRequestID, out var phone))
            {
                return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
            }

            string reply;
            //string phone;

            if (mpesaCallback.ResultCode == 0)
            {
                var items = mpesaCallback.CallbackMetadata?.Item ?? new();
                var receipt =items.FirstOrDefault(i=>i.Name == "MpesaReceiptNumber")?.Value?.GetString();
                var amount =items.FirstOrDefault(i=>i.Name == "Amount")?.Value?.GetDecimal();

                _logger.LogWarning("STK callback items: {items}",items);

                _logger.LogWarning("Phone is:{phone} ", phone ?? "NOT FOUND!!");
                _logger.LogWarning("Receipt is: {receipt}", receipt);
                _logger.LogWarning("Amount is: {amount}", amount);
                reply = $"Payment of Ksh {amount} received. Receipt: {receipt}. Thank you for shopping with Prime Hub!";

                //var successMessage = new
                //{
                //    messaging_product = "whatsapp",
                //    to = phone,
                //    type = "text",
                //    text = new { body = reply }
                //};

                //await whatsAppMessage.SendMessage(successMessage);

            }
            else
            {
                //string resultDesc =stkCallback.GetProperty("ResultDesc").GetString()!;
                _logger.LogWarning("Not successful: {resultDesc}",mpesaCallback.ResultDesc);

                reply = mpesaCallback.ResultCode switch
                {
                    1032 => "You cancelled the payment request.",
                    1 => "Payment failed: insufficient M-Pesa balance.",
                    2001 => "Payment failed: wrong PIN entered.",
                    1037 => "The payment request timed out. Please try again.",
                    _ => $"Payment failed: {mpesaCallback.ResultDesc}"
                };
            }

            var successMessage = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "text",
                text = new { body = reply }
            };

            await whatsAppMessage.SendMessage(successMessage);

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
