using whatsapp_cloud_api.Models;

namespace whatsapp_cloud_api.Services.IServices
{
    public interface IConversationState
    {
        ConversationState GetState(string phone);
        void SaveState(ConversationState state);
        void ClearState(string phone);

        ConversationState? FindCheckoutById(string checkoutId);


    }
}
