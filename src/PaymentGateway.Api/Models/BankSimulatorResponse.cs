using System;

namespace PaymentGateway.Models
{
    public class BankSimulatorResponse
    {
        public bool Authorized { get; set; }
        public string AuthorizationCode { get; set; }
    }
}