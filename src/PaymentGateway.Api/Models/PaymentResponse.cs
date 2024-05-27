namespace PaymentGateway.Models
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public StatusEnum Status { get; set; }
        public string Last4CardDigits { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public CurrencyCodes Currency { get; set; }
        public int Amount { get; set; }

        public PaymentResponse(Guid id, StatusEnum status, string last4CardDigits, int expiryMonth, int expiryYear, CurrencyCodes currency, int amount)
        {
            Id = id;
            Status = status;
            Last4CardDigits = last4CardDigits;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            Currency = currency;
            Amount = amount;
        }
    }

    public enum StatusEnum {
        Authorized,
        Declined,
        Rejected // should return if invalid info supplied
    };

    public enum CurrencyCodes {
        USD,
        EUR,
        GBP
    };
}