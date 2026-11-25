namespace whatsapp_cloud_api.Services.IServices
{
    public interface IPendingPaymentStore
    {
        void Add(string checkoutRequestId,string phoneNumber);
        bool Tryremove(string checkoutRequestID,out string phoneNumber);
    }
}
