namespace whatsapp_cloud_api.Services.IServices
{
    public interface IWhatsAppMessage
    {
        Task<HttpResponseMessage> SendMessage(object message);
    }
}
