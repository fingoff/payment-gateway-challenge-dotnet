using System;
using System.Collections.Generic;

namespace PaymentGateway.Models
{
    public static class InMemoryPaymentStore
    {
        // Dictionary to hold the payment details with payment ID as the key
        private static readonly Dictionary<Guid, PaymentResponse> Payments = new() 
        {
            { 
                Guid.Parse("f32bd16f-e103-425e-9fa8-faa4accc3b93"),
                new PaymentResponse(
                    Guid.Parse("f32bd16f-e103-425e-9fa8-faa4accc3b93"),
                    StatusEnum.Authorized,
                    1234,
                    12,
                    2027,
                    CurrencyCodes.USD,
                    1000
                )
            }
        };

        // Method to save payment details
        public static void SavePayment(PaymentResponse paymentResponse)
        {
            Payments[paymentResponse.Id] = paymentResponse;
        }

        // Method to retrieve payment details by ID
        public static PaymentResponse? TryGetPayment(Guid id)
        {
            Payments.TryGetValue(id, out var paymentResponse);
            return paymentResponse;
        }
    }
}