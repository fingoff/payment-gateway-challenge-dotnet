using System.ComponentModel.DataAnnotations;
using PaymentGateway.Attributes;

namespace PaymentGateway.Models
{
    public class PaymentRequest
    {
        [Required]
        [StringLength(19, MinimumLength = 14)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Card number must be numeric.")]
        public string CardNumber { get; set; }

        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Required]
        [Range(1, 9999)]
        [FutureExpiryDate("ExpiryMonth")]
        public int ExpiryYear { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public int Amount { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "CVV must be numeric.")]
        public string CVV { get; set; }
    }
}