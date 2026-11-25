namespace whatsapp_cloud_api.Models
{
    public enum ConversationStep
    {
        Idle,
        ViewingProducts,
        ConfirmingPaymentNumber,
        AwaitingPaymentNumber,
        AwaitingPayment
    }
    public class ConversationState
    {
        public string PhoneNumber { get; set; } = "";
        public ConversationStep Step { get; set; } = ConversationStep.Idle;
        public string? ProductId { get; set; }
        public string? Productname { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentNumber { get; set; }
        public string? CheckoutRequestID { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
