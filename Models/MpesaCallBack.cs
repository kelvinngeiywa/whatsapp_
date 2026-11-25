using System.Text.Json;

namespace whatsapp_cloud_api.Models
{
    public class MpesaCallBack
    {
        public CallbackBody? Body { get; set; }
    }

    public class CallbackBody
    {
        public StkCallback? StkCallback { get; set; }
    }

    public class StkCallback
    {
        public string MerchantRequestID { get; set; } = "";
        public string CheckoutRequestID { get; set; } = "";
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; } = "";
        public CallbackMetadata? CallbackMetadata { get; set; }

    }

    public class CallbackMetadata
    {
        public List<CallbackItem> Item { get; set; } = new();
    }

    public class CallbackItem
    {
        public string Name { get; set; } = "";
        public JsonElement? Value { get; set; } 
    }
}
