using System.Collections.Concurrent;
using whatsapp_cloud_api.Models;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class InMemoryConversationState : IConversationState
    {
        private readonly ConcurrentDictionary<string, ConversationState> _states = new();
        public void ClearState(string phone) => _states.TryRemove(phone, out _);


        public ConversationState? FindCheckoutById(string checkoutId) =>
            _states.Values.FirstOrDefault(s=>s.CheckoutRequestID == checkoutId);


        public ConversationState GetState(string phone) =>
            _states.GetOrAdd(phone,p =>new ConversationState { PhoneNumber = p});
        

        public void SaveState(ConversationState state)
        {
            state.LastUpdated= DateTime.UtcNow;
            _states[state.PhoneNumber]= state;
        }
    }
}
