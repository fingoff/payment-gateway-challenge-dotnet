using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
         private readonly HttpClient _httpClient;

        public PaymentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Card number	Required	
        // Between 14-19 characters long	
        // Must only contain numeric characters	

        // Expiry month	Required	
        // Value must be between 1-12

        // Expiry year	Required	
        // Value must be in the future	Ensure the combination of expiry month + year is in the future

        // Currency	Required	Refer to the list of ISO currency codes. Ensure your submission validates against no more than 3 currency codes
        // Must be 3 characters	

        // Amount	Required	Represents the amount in the minor currency unit. For example, if the currency was USD then
        // $0.01 would be supplied as 1
        // $10.50 would be supplied as 1050
        // Must be an integer	

        // CVV	Required	
        // Must be 3-4 characters long	
        // Must only contain numeric characters

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create the JSON object as per the required format for bank simulator
            var paymentData = new
            {
                card_number = request.CardNumber,
                expiry_date = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}", // Format month and year
                currency = request.Currency,
                amount = request.Amount,
                cvv = request.CVV
            };

            // serialize data to send to Bank simulator
            string paymentJson = JsonSerializer.Serialize(paymentData);

            HttpResponseMessage response;
            try
            {
                var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync("http://localhost:8080/payments", content);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error from Bank Simulator.");
                }
            }
            catch (HttpRequestException)
            {
                return BadRequest("Error communicating with Bank Simulator.");
            }

            // Read and deserialize the response
            string jsonResponse = await response.Content.ReadAsStringAsync();
            BankSimulatorResponse bankResponse;
            try
            {
                bankResponse = JsonSerializer.Deserialize<BankSimulatorResponse>(jsonResponse);
                if (bankResponse == null)
                {
                    throw new JsonException("Deserialized response is null.");
                }
            }
            catch (JsonException)
            {
                return BadRequest("Invalid response from Bank Simulator.");
            }

            // Create the payment response
            Guid id = Guid.NewGuid();
            StatusEnum status = bankResponse.Authorized ? StatusEnum.Authorized : StatusEnum.Declined;
            string last4CardDigits = request.CardNumber.Substring(request.CardNumber.Length - 4);
            CurrencyCodes currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), request.Currency, true);
            
            PaymentResponse paymentResponse = new(id, status, last4CardDigits, request.ExpiryMonth, request.ExpiryYear, currency, request.Amount);

            // Save payment details to in-memory store
            InMemoryPaymentStore.SavePayment(paymentResponse);

            return Ok(paymentResponse);
        }

        [HttpGet]
        public async Task<IActionResult> RetrievePaymentDetails(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get Payment Details with Id from simulated DB
            PaymentResponse? paymentResponse = InMemoryPaymentStore.TryGetPayment(id);
            if (paymentResponse == null)
            {
                return NotFound("Payment not found.");
            }

            return Ok(paymentResponse);
        }
    }
}