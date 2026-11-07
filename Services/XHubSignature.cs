using System.Security.Cryptography;
using System.Text;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class XHubSignature(ILogger<XHubSignature> _logger,IConfiguration configuration) : IHubSignature
    {
        private readonly string signature = configuration["ConnectionStrings:AppSecret"];
        public async Task<string> SignAsync(Stream body)
        {
           using (var memoryStream=new MemoryStream())
            {
                await body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var payLoadBytes = memoryStream.ToArray();

                using (var hmc = new HMACSHA256(Encoding.UTF8.GetBytes(signature)))
                {
                    var hash = hmc.ComputeHash(payLoadBytes);
                    var xhubSignature = $"sha256={BitConverter.ToString(hash).Replace("-", "").ToLower()}";

                    return xhubSignature;
                }
            }
        }
    }
}
