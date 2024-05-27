using System;
using System.ComponentModel.DataAnnotations;    
    
namespace PaymentGateway.Attributes
{
    public class FutureExpiryDateAttribute : ValidationAttribute
    {
        private readonly string _expiryMonth;

        public FutureExpiryDateAttribute(string expiryMonth)
        {
            _expiryMonth = expiryMonth;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var expiryYear = (int)value;

            var expiryMonthProperty = validationContext.ObjectType.GetProperty(_expiryMonth);

            if (expiryMonthProperty == null)
                throw new ArgumentException("Property with this name not found");

            var expiryMonth = (int)expiryMonthProperty.GetValue(validationContext.ObjectInstance);

            int lastDay = DateTime.DaysInMonth(expiryYear, expiryMonth);
            DateTime expiryDate = new(expiryYear, expiryMonth, lastDay);

            if (expiryDate >= DateTime.Now)
                return ValidationResult.Success;
            else
                return new ValidationResult("Expiry date must be in the future.");
        }
    }
}