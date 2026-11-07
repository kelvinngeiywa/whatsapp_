namespace whatsapp_cloud_api.Services.IServices
{
    public interface IHubSignature
    {
        Task<string> SignAsync(Stream body);
    }
}
