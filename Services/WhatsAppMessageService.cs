using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class WhatsAppMessageService(HttpClient httpClient,IConfiguration configuration) : IWhatsAppMessage
    {
        private readonly string accessToken = configuration["ConnectionStrings:AccessToken"];
        private readonly string apiVersion = configuration["ConnectionStrings:ApiVersion"];
        private readonly string phoneNumberId = configuration["ConnectionStrings:PhoneNumberId"];

        public async Task<HttpResponseMessage> SendMessage(object message)
        {
            return await SendMessageInternal(phoneNumberId,message);
        }

        private async Task<HttpResponseMessage> SendMessageInternal(string numberId,object data)
        {
            var url = $"https://graph.facebook.com/{apiVersion}/{numberId}/messages";

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                AuthenticationHeaderValue("Bearer", accessToken);
            return await httpClient.PostAsJsonAsync(url, data);
        }
    }
}
