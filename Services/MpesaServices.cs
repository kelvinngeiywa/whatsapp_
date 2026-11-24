using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class MpesaServices: IMpesa
    {
        private readonly HttpClient _httpClient;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly IConfiguration _configuration;
        public MpesaServices(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://sandbox.safaricom.co.ke");
           
            _configuration = configuration;
            _consumerKey = _configuration.GetValue<string>("MpesaDaraja:ConsumerKey")!;
            _consumerSecret = _configuration.GetValue<string>("MpesaDaraja:ConsumerSecret")!;
        }
        public async Task<string> InitiateSTKPushAsync(string phoneNumber, decimal amount, string callBackUrl)
        {
            var token = await GetAccessTokenAsync();

            var shortCode = _configuration.GetValue<string>("MpesaDaraja:ShortCode");

            var passKey = _configuration.GetValue<string>("MpesaDaraja:PassKey");

            var timeStamp = DateTime.UtcNow.AddHours(3).ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

            var passwordBytes = Encoding.UTF8.GetBytes($"{shortCode}{passKey}{timeStamp}");

            var password=Convert.ToBase64String(passwordBytes);

            var payload = new
            {
                BusinessShortCode=shortCode,
                Password=password,
                Timestamp=timeStamp,
                TransactionType="CustomerPayBillOnline",
                Amount=(int)amount,
                PartyA=phoneNumber,
                PartyB=shortCode,
                PhoneNumber=phoneNumber,
                CallBackURL=callBackUrl,
                AccountReference="Primehub123",
                TransactionDesc="Payment for order 123"
            };

            var request = new HttpRequestMessage(HttpMethod.Post,"/mpesa/stkpush/v1/processrequest");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",token);

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response=await _httpClient.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var authBytes = Encoding.UTF8.GetBytes($"{_consumerKey}:{_consumerSecret}");
            var authHeader =Convert.ToBase64String(authBytes);

            var request =new HttpRequestMessage(HttpMethod.Get,"/oauth/v1/generate?grant_type=client_credentials");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response= await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var content=await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);

            return doc.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
