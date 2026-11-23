namespace whatsapp_cloud_api.Services.IServices
{
    public interface IMpesa
    {
        Task<string> InitiateSTKPushAsync(string phoneNumber,decimal amount,string callBackUrl); 
    }
}
