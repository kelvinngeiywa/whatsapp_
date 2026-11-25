using System.Collections.Concurrent;
using whatsapp_cloud_api.Services.IServices;

namespace whatsapp_cloud_api.Services
{
    public class PendingPaymentStore : IPendingPaymentStore
    {
        private readonly ConcurrentDictionary<string, string> _pending = new();
        public void Add(string checkoutRequestId, string phoneNumber)
        {
            _pending[checkoutRequestId] = phoneNumber;
        }

        public bool Tryremove(string checkoutRequestID, out string phoneNumber) =>  _pending.TryRemove(checkoutRequestID,out phoneNumber!);
        
    }
}
