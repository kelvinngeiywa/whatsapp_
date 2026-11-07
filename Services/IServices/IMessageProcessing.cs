using System.Text.Json;

namespace whatsapp_cloud_api.Services.IServices
{
    public interface IMessageProcessing
    {
        Task ProcessMesssage(JsonElement message, JsonElement contact);
    }
}
